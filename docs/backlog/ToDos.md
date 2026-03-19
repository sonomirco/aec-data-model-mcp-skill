Here is a list of ToDos:

- [x] Fix MCP inspector connection issue (completed March 18, 2026): use `Transport Type = streamable HTTP` with server URL `https://localhost:7270/` (or `http://localhost:5096/`). Legacy SSE endpoint `/sse` remains available for clients that require it.

- [] Review the prompt.poml structure with this suggested structure https://github.com/anthropics/courses/blob/b4f26aedef55e06ad5eead5de83985249d1fab2f/prompt_engineering_interactive_tutorial/Anthropic%201P/09_Complex_Prompts_from_Scratch.ipynb

- [] Eval the tool following this example - https://github.com/anthropics/anthropic-cookbook/blob/main/tool_evaluation/tool_evaluation.ipynb

- [] Eval the list of generated questions

- [] Pass the schema as part of the prompt for advanced queries.

- [] Pagination needs to be recursive once we have a cursor value and hasNextPage is still true.

- Run Codex to ultra thing and create an AGENTS.md file for each projet.
- Needs to review the tests and see if we can clean them.
- Refactor and clean the code.
- Find a process of development that use TDD.

## 1. Improve the LLM as a judge to be used to evaluate the RSQL queries.

Here is an initial prompt.

You are an RSQL syntax validation expert tasked with reviewing automated test results.

## Role and Objective
Confirm whether two provided RSQL expressions are functionally equivalent, specifically considering minor formatting and range logic differences.

## Instructions
- Evaluate both expressions for:
  1. Semantic Equivalence: Do they select the same set of filtered elements?
  2. RSQL Validity: Are both expressions syntactically correct?
  3. Range Logic: Assess the difference between '< 200' and '<= 200'.
  4. Formatting: Consider if operator spacing changes the result.

After your evaluation, validate that your output strictly matches the required output structure and provides a clear, brief justification relevant to the automated test context.

## Context
- The expressions filter AEC elements (e.g., architectural walls) using an RSQL query on area.
- Automatic RSQL generation should be robust to minor formatting changes.

## Decision Criteria
- Return **PASS** if the expressions are semantically identical and only differ in formatting.
- Return **FAIL** for any significant logical or syntactic error.
- Return **MINOR_DIFFERENCE** for valid but slightly different edge case range handling (e.g., inclusive vs exclusive boundary).
- If either expression fails RSQL syntax, explain and set VERDICT to FAIL.

## Output Format
Respond with:
```json
{"VERDICT":"[PASS|FAIL|MINOR_DIFFERENCE]","REASONING":"<brief justification>","PREFERRED":"expected|actual|either"}
```
Output compact JSON (no line breaks or extra spaces) for automation compatibility. Ensure response fully matches this format before submission.

## Design Principles
1. Make context clear for user intent interpretation.
2. Distinguish clearly between semantic and formatting checks.
3. Reflect domain knowledge (RSQL and filtering for AEC elements).
4. Provide nuanced options for ambiguous or edge cases.
5. Output compact and structured responses for automation.

## Examples

- PASS : expected 'property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.length>0.4' , got 'property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.length>0.4'

- PASS : expected 'property.name.category==doors and 'property.name.Element Context'==Instance and 'property.name.Family Name'>'Solid Timber'' , got 'property.name.category==doors and 'property.name.Element Context'==Instance and 'property.name.Family Name'>'Solid Timber''

- FAIL : expected 'property.name.category=contains='pipe fittings' and 'property.name.Element Context'==Instance' , got 'property.name.category=contains=Pipe Fittings and 'property.name.Element Context'==Instance'

- FAIL : expected 'property.name.category=contains=doors and 'property.name.Element Context'==Instance' , got 'property.name.category=contains=Doors and property.name.Element Context==Instance'

- MINOR_DIFFERENCE : expected 'property.name.category=contains='pipe fittings' and 'property.name.Element Context'==Instance' , got 'property.name.category=contains='Pipe Fittings' and 'property.name.Element Context'==Instance'

- MINOR_DIFFERENCE : expected 'property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.area>=100 and property.name.area<200', got 'property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.area>=100 and property.name.area<=200'
