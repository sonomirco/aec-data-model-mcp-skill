import os
import json
import re
import sys
from typing import Dict, Any, List
from openai import OpenAI
from dotenv import load_dotenv
from poml import poml
from test_cases import get_test_cases
from template_config import TEMPLATE_CONFIGS, VIEWER_TOOL_CONFIGS, get_all_template_names, get_all_tool_names
from results_formatter import EvaluationResultsFormatter, format_evaluation_results

PROMPT_PATH = "prompt.poml"

def eval_prompt(json_output: bool = False) -> str:
    """
    Run POML evaluation tests - main entry point for CSnakes
    
    Args:
        json_output: If True, output raw JSON; if False, use Rich formatting
    
    Returns:
        Dictionary containing evaluation results
    """
    load_dotenv()
    
    # Initialize OpenAI client
    api_key = os.getenv("OPENAI_API_KEY")
    if not api_key:
        return {
            "error": "OPENAI_API_KEY not found in environment variables",
            "success": False
        }
    
    client = OpenAI(api_key=api_key)
    
    # Get test cases from external file
    test_cases = get_test_cases()
    
    results = []
    passed = 0
    total = len(test_cases)

    # Load POML file and pass it as a simple text file
    prompt = _raw_prompt()
    
    # Initialize formatter for progress display
    formatter = EvaluationResultsFormatter()
    
    try:
        for i, test_case in enumerate(test_cases):
            # Display progress using Rich formatter
            formatter.display_progress(i + 1, total, test_case['description'])
            
            # Check if this is a sequence test
            if "sequence" in test_case:
                evaluation = _evaluate_sequence(client, prompt, test_case)
            else:
                # Regular single test case
                evaluation = _evaluate_single_test(client, prompt, test_case)
            
            evaluation["description"] = test_case["description"]
            
            if evaluation["pass"]:
                passed += 1
            
            results.append(evaluation)
        
        pass_rate=(passed/total)*100
        print_prompt(pass_rate)
        
        results_data = {
            "success": True,
            "total_tests": total,
            "passed": passed,
            "failed": total - passed,
            "pass_rate": f"{pass_rate:.1f}%",
            "results": results
        }
        
        results_json = json.dumps(results_data)
        
        # Display formatted results unless JSON output is requested
        if not json_output:
            print("\n")  # Add some space after progress messages
            format_evaluation_results(results_json)
        
        return results_json
        
    except Exception as e:
        return {
            "success": False,
            "error": str(e)
        }
    
def print_prompt(rate: float):
    """Export the POML prompt to markdown if pass rate is high enough"""
    # Use absolute path and proper path separator for cross-platform compatibility
    import os
    base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    path = os.path.join(base_dir, 'apsMcp.Tools', 'prompt.md')
    
    prompt = _raw_prompt()
    if rate > 80.0:  # Use 80.0% instead of 0.80 for clarity
        try:
            # Ensure directory exists
            os.makedirs(os.path.dirname(path), exist_ok=True)
            
            with open(path, "w", encoding='utf-8') as f:
                f.write(prompt)
            print(f'✓ Exported prompt to {path} (pass rate: {rate:.1f}%)')
        except Exception as e:
            print(f'✗ Failed to export prompt: {e}')
    else:
        print(f'• Pass rate {rate:.1f}% below 80% threshold - not exporting prompt')

def _generate_response_schema() -> dict:
    """Generate JSON schema dynamically from template configuration"""
    template_names = get_all_template_names() + ["null"]  # Add null for viewer tools
    tool_names = get_all_tool_names()
    
    return {
        "type": "object",
        "properties": {
            "mcp_call": {
                "type": "object",
                "properties": {
                    "tool": {
                        "type": "string",
                        "enum": tool_names
                    },
                    "template": {
                        "type": ["string", "null"],
                        "enum": template_names
                    },
                    "parameters": {
                        "type": "array",
                        "items": {"type": "string"},
                        "description": "Array of parameters (empty array for no parameters)"
                    }
                },
                "required": ["tool", "template", "parameters"],
                "additionalProperties": False
            }
        },
        "required": ["mcp_call"],
        "additionalProperties": False
    }

def _evaluate_single_test(client: OpenAI, prompt: str, test_case: Dict[str, Any]) -> Dict[str, Any]:
    """Evaluate a single test case"""
    # Define JSON schema for structured output (dynamically generated)
    response_schema = _generate_response_schema()
    
    # Get assistant response with structured output
    response = client.chat.completions.create(
        model="gpt-4o-mini",
        messages=[
            {"role": "system", "content": prompt},
            {"role": "user", "content": test_case["input"]}],
        temperature=0.5,
        response_format={
            "type": "json_schema",
            "json_schema": {
                "name": "mcp_decision",
                "schema": response_schema,
                "strict": True
            }
        }
    )
    
    assistant_response = response.choices[0].message.content
    
    try:
        # Parse JSON response
        response_json = json.loads(assistant_response)
        mcp_call = response_json.get("mcp_call", {})
        
        # Evaluate MCP decision
        evaluation = _evaluate_mcp_decision(
            test_case["input"],
            mcp_call,
            test_case["expected"],
            client
        )
        evaluation["raw_response"] = assistant_response
        evaluation["tokens_used"] = response.usage.total_tokens
        
    except json.JSONDecodeError as e:
        evaluation = {
            "pass": False,
            "issues": [f"Invalid JSON response: {str(e)}"],
            "raw_response": assistant_response,
            "tokens_used": response.usage.total_tokens
        }
    
    return evaluation

def _evaluate_sequence(client: OpenAI, prompt: str, test_case: Dict[str, Any]) -> Dict[str, Any]:
    """Evaluate a sequence test case with context"""
    sequence = test_case["sequence"]
    all_issues = []
    total_tokens = 0
    context_data = {}
    
    for step_index, step in enumerate(sequence):
        # Use formatter for sequence step progress
        formatter = EvaluationResultsFormatter()
        formatter.console.print(f"    [dim]Step {step_index + 1}/{len(sequence)}[/dim]: {step['input']}")
        
        # Evaluate this step
        step_evaluation = _evaluate_single_test(client, prompt, step)
        total_tokens += step_evaluation.get("tokens_used", 0)
        
        if not step_evaluation["pass"]:
            all_issues.extend([f"Step {step_index + 1}: {issue}" for issue in step_evaluation["issues"]])
        else:
            # For this simple implementation, mock context handling
            # In step 1, if we get GetElementGroupsByProject, store a mock elementGroupId
            if step_index == 0 and step["expected"].get("template") == "GetElementGroupsByProject":
                context_data["first_model_id"] = "MOCK_ELEMENT_GROUP_ID"
            
            # In step 2, check if the expected parameters reference the first model
            if step_index == 1 and "FIRST_MODEL_ID" in str(step["expected"].get("parameters", [])):
                # Replace FIRST_MODEL_ID with our mock value in the expectation
                expected_with_context = step["expected"].copy()
                if "FIRST_MODEL_ID" in expected_with_context["parameters"]:
                    param_index = expected_with_context["parameters"].index("FIRST_MODEL_ID")
                    expected_with_context["parameters"][param_index] = context_data.get("first_model_id", "UNKNOWN")
                
                # Re-evaluate with correct context
                actual_mcp_call = json.loads(step_evaluation["raw_response"]).get("mcp_call", {})
                context_evaluation = _evaluate_mcp_decision(
                    step["input"],
                    actual_mcp_call,
                    expected_with_context,
                    client
                )
                
                if not context_evaluation["pass"]:
                    all_issues.extend([f"Step {step_index + 1} context: {issue}" for issue in context_evaluation["issues"]])
    
    return {
        "pass": len(all_issues) == 0,
        "issues": all_issues,
        "tokens_used": total_tokens,
        "sequence_steps": len(sequence)
    }

def _raw_prompt()->str:
    raw_prompt = poml(PROMPT_PATH, chat=False, format='raw')
    prompt_data = json.loads(raw_prompt)
    return prompt_data.get("messages", "")

def _validate_tool_and_template(result: Dict[str, Any], mcp_call: Dict[str, Any], expected: Dict[str, Any]) -> bool:
    """Validate tool and template selection"""
    actual_tool = mcp_call.get("tool")
    expected_tool = expected.get("tool")
    
    if actual_tool != expected_tool:
        result["issues"].append(f"Wrong tool: expected '{expected_tool}', got '{actual_tool}'")
        return False
    
    actual_template = mcp_call.get("template")
    expected_template = expected.get("template")
    
    # Handle null/None template comparison for viewer tools
    if expected_template is None:
        if actual_template is not None and actual_template != "null":
            result["issues"].append(f"Wrong template: expected 'None', got '{actual_template}'")
            return False
    elif actual_template != expected_template:
        result["issues"].append(f"Wrong template: expected '{expected_template}', got '{actual_template}'")
        return False
    
    return True


def _validate_template_parameters(result: Dict[str, Any], template_name: str, actual_params: Any, expected_params: Any, client: OpenAI) -> bool:
    """Generic parameter validation using template configuration"""
    template_config = TEMPLATE_CONFIGS.get(template_name)
    if not template_config:
        result["issues"].append(f"Unknown template: {template_name}")
        return False
    
    expected_count = template_config["parameter_count"]
    param_names = template_config["parameters"]
    
    # Handle missing parameter case
    if expected_params == "required_but_missing":
        if isinstance(actual_params, list) and len(actual_params) == 0:
            result["pass"] = True
            result["note"] = f"Correctly identified {template_name} needs {', '.join(param_names)} parameter(s)"
            return True
        else:
            result["issues"].append(f"Should not provide parameters when they're not in the input")
            return False
    
    # Handle parameter validation
    if not isinstance(actual_params, list):
        result["issues"].append(f"{template_name} parameters should be an array")
        return False
    
    # Check parameter count - handle variable parameter templates
    if template_config.get("variable_parameters", False):
        # Variable parameter template (e.g., GetElementsByProperties)
        min_params = template_config.get("min_parameters", expected_count)
        if len(actual_params) < min_params:
            param_desc = ', '.join(param_names)
            result["issues"].append(f"{template_name} requires at least {min_params} parameter(s): {param_desc}")
            return False
    else:
        # Fixed parameter template
        if len(actual_params) != expected_count:
            if expected_count == 0:
                result["issues"].append(f"{template_name} should have empty parameters array")
            else:
                param_desc = ', '.join(param_names)
                result["issues"].append(f"{template_name} requires {expected_count} parameter(s): {param_desc}")
            return False
    
    # Check parameter values if expected values are provided
    if isinstance(expected_params, list) and len(expected_params) > 0:
        for i, (actual, expected, name) in enumerate(zip(actual_params, expected_params, param_names)):
            # Special case: category parameters should use LLM-based semantic validation
            if name.lower() == "category":
                if not _is_category_equivalent(str(actual), str(expected), client):
                    result["issues"].append(f"Wrong {name}: expected '{expected}', got '{actual}'")
                    return False
            else:
                # Regular exact match for other parameters
                if actual != expected:
                    result["issues"].append(f"Wrong {name}: expected '{expected}', got '{actual}'")
                    return False
    
    return True


def _validate_viewer_render_params(result: Dict[str, Any], actual_params: Any, expected_params: list) -> bool:
    """Validate aps-viewer-render parameters (requires fileVersionUrn)"""
    if not isinstance(actual_params, list) or len(actual_params) != 1:
        result["issues"].append("aps-viewer-render requires exactly one fileVersionUrn parameter")
        return False
    
    actual_urn = actual_params[0]
    expected_urn = expected_params[0] if expected_params else None
    
    if expected_urn and actual_urn != expected_urn:
        result["issues"].append(f"Wrong fileVersionUrn: expected '{expected_urn}', got '{actual_urn}'")
        return False
    
    # Validate URN format
    if not actual_urn.startswith("urn:adsk.wipprod:fs.file:vf."):
        result["issues"].append(f"Invalid fileVersionUrn format: should start with 'urn:adsk.wipprod:fs.file:vf.', got '{actual_urn}'")
        return False
    
    return True

def _evaluate_mcp_decision(user_input: str, mcp_call: Dict[str, Any], expected: Dict[str, Any], client: OpenAI) -> Dict[str, Any]:
    """
    Rule-based evaluation for MCP decision validation
    
    Args:
        user_input: User's request
        mcp_call: The MCP call structure from LLM response
        expected: Expected MCP decision structure
    
    Returns:
        Evaluation result with pass/fail
    """
    result = {
        "user_input": user_input,
        "expected": expected,
        "actual_mcp_call": mcp_call,
        "pass": False,
        "issues": []
    }
    
    # Validate tool and template
    if not _validate_tool_and_template(result, mcp_call, expected):
        return result
    
    # Parameter validation
    actual_params = mcp_call.get("parameters")
    expected_params = expected.get("parameters")
    expected_template = expected.get("template")
    expected_tool = expected.get("tool")
    
    # Handle GraphQL templates using generic validation
    if expected_template in TEMPLATE_CONFIGS:
        if not _validate_template_parameters(result, expected_template, actual_params, expected_params, client):
            return result
    
    # Handle viewer tools (no template, but require parameters)
    elif expected_tool == "aps-viewer-render":
        if not _validate_viewer_render_params(result, actual_params, expected_params):
            return result
    elif expected_tool == "aps-highlight-elements":
        # For now, just validate that we have parameters (can be extended later)
        if not isinstance(actual_params, list) or len(actual_params) == 0:
            result["issues"].append("aps-highlight-elements requires External ID parameters")
            return result
    
    # If we reach here, validation passed
    result["pass"] = True
    return result

# Global cache for category equivalence checks to avoid duplicate API calls
_category_cache = {}

def _is_category_equivalent(actual: str, expected: str, client: OpenAI) -> bool:
    """
    Check if two categories are semantically equivalent using GPT-4o-mini.
    Uses caching to avoid duplicate API calls.
    """
    # Normalize inputs
    actual_clean = actual.strip().lower()
    expected_clean = expected.strip().lower()
    
    # Exact match (case insensitive)
    if actual_clean == expected_clean:
        return True
    
    # Check cache (bidirectional)
    cache_key1 = f"{actual_clean}|{expected_clean}"
    cache_key2 = f"{expected_clean}|{actual_clean}"
    
    if cache_key1 in _category_cache:
        return _category_cache[cache_key1]
    if cache_key2 in _category_cache:
        return _category_cache[cache_key2]
    
    message = f"Are '{actual}' and '{expected}' semantically equivalent when ignoring case differences and singular/plural variations? Answer only 'yes' or 'no'"

    try:
        # Call GPT-4o-mini for semantic equivalence
        response = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[{
                "role": "user", 
                "content": message
            }],
            temperature=0,
            max_tokens=10
        )
        
        answer = response.choices[0].message.content.strip().lower()
        is_equivalent = "yes" in answer
        
        # Cache the result (bidirectional)
        _category_cache[cache_key1] = is_equivalent
        _category_cache[cache_key2] = is_equivalent
        
        return is_equivalent
        
    except Exception as e:
        # Fallback to exact match if API fails
        return actual_clean == expected_clean

if __name__ == "__main__":
    # Parse command line arguments
    json_output = "--json" in sys.argv
    
    print("Starting POML evaluation...")
    result = eval_prompt(json_output=json_output)
    
    # If JSON output is requested, print the raw JSON
    if json_output:
        print("\nEvaluation Results:")
        print(result)
