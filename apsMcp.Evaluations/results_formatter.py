"""
Rich table formatter for evaluation results
"""
import json
from typing import Dict, Any, List
from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich.layout import Layout
from rich.text import Text
from rich import box
from rich.progress import Progress, BarColumn, TextColumn, TimeRemainingColumn


class EvaluationResultsFormatter:
    """Format evaluation results using Rich tables and panels"""
    
    def __init__(self):
        self.console = Console(force_terminal=True)
    
    def format_results(self, results_json: str) -> None:
        """Format and display evaluation results"""
        try:
            results = json.loads(results_json)
        except json.JSONDecodeError:
            self.console.print("[red]Error: Invalid JSON results[/red]")
            return
        
        if not results.get("success"):
            self._display_error(results.get("error", "Unknown error"))
            return
        
        # Display detailed test results
        self._display_test_results(results.get("results", []))
        
        # Display detailed failure information only
        self._display_failure_details(results.get("results", []))
    
    def _display_summary(self, results: Dict[str, Any]) -> None:
        """Display evaluation summary with pass rate and statistics"""
        
        # Create summary table
        summary_table = Table(title="[bold blue]Evaluation Summary[/bold blue]", box=box.ROUNDED)
        summary_table.add_column("Metric", style="cyan", width=15)
        summary_table.add_column("Value", style="bold", width=20)
        
        total_tests = results.get("total_tests", 0)
        passed = results.get("passed", 0)
        failed = results.get("failed", 0)
        pass_rate = results.get("pass_rate", "0%")
        
        # Color code pass rate based on value
        pass_rate_color = self._get_pass_rate_color(pass_rate)
        
        summary_table.add_row("Total Tests", str(total_tests))
        summary_table.add_row("Passed", f"[green]{passed} [green]PASS[/green][/green]")
        summary_table.add_row("Failed", f"[red]{failed} [red]FAIL[/red][/red]")
        summary_table.add_row("Pass Rate", f"[{pass_rate_color}]{pass_rate}[/{pass_rate_color}]")
        
        self.console.print(summary_table)
        self.console.print()
    
    def _display_test_results(self, test_results: List[Dict[str, Any]]) -> None:
        """Display detailed test results in a table"""
        
        results_table = Table(title="[bold green]Detailed Test Results[/bold green]", box=box.HEAVY_EDGE, expand=True)
        results_table.add_column("Status", width=8, justify="center")
        results_table.add_column("Test Description", style="dim", min_width=25, max_width=50, overflow="fold")
        results_table.add_column("Issues", style="red", min_width=30, overflow="fold")
        results_table.add_column("Tokens", justify="right", width=8)
        results_table.add_column("Type", style="blue", width=12)
        
        for result in test_results:
            # Status with color coding
            status = "PASS" if result.get("pass") else "FAIL"
            status_style = "[green]PASS[/green]" if result.get("pass") else "[red]FAIL[/red]"
            
            # Keep full descriptions - Rich will wrap them
            description = result.get("description", "N/A")
            
            # Format issues - show all issues without truncation
            issues = result.get("issues", [])
            if issues:
                # Join multiple issues with line breaks for better readability
                issues_text = "\n".join(f"• {issue}" for issue in issues)
            else:
                issues_text = "[dim]No issues[/dim]"
            
            # Token usage
            tokens = str(result.get("tokens_used", 0))
            
            # Determine test type
            test_type = self._get_test_type(result)
            
            results_table.add_row(
                status_style,
                description,
                issues_text,
                tokens,
                test_type
            )
        
        self.console.print(results_table)
        self.console.print()
    
    def _display_failure_analysis(self, test_results: List[Dict[str, Any]]) -> None:
        """Display analysis of failures grouped by common issues"""
        
        failed_tests = [test for test in test_results if not test.get("pass")]
        if not failed_tests:
            return
        
        # Group failures by issue type
        issue_groups = {}
        for test in failed_tests:
            for issue in test.get("issues", []):
                # Extract issue type (first few words)
                issue_type = " ".join(issue.split()[:3])
                if issue_type not in issue_groups:
                    issue_groups[issue_type] = []
                issue_groups[issue_type].append(test.get("description", "Unknown"))
        
        # Create failure analysis table
        failure_table = Table(title="[bold red]Failure Analysis[/bold red]", box=box.DOUBLE_EDGE, expand=True)
        failure_table.add_column("Issue Type", style="red bold", min_width=20, max_width=40, overflow="fold")
        failure_table.add_column("Count", justify="center", width=8)
        failure_table.add_column("Affected Tests", style="dim", min_width=30, overflow="fold")
        
        for issue_type, affected_tests in sorted(issue_groups.items(), key=lambda x: len(x[1]), reverse=True):
            count = len(affected_tests)
            
            # Show all affected tests - Rich will wrap them
            test_list = "\n".join(f"• {test}" for test in affected_tests)
            
            failure_table.add_row(
                issue_type,
                str(count),
                test_list
            )
        
        self.console.print(failure_table)
        self.console.print()
    
    def _display_failure_details(self, test_results: List[Dict[str, Any]]) -> None:
        """Display detailed failure information including full LLM responses"""
        
        failed_tests = [test for test in test_results if not test.get("pass")]
        if not failed_tests:
            return
        
        self.console.print(Panel(
            f"[bold red]Detailed Failure Information[/bold red]\n\n"
            f"Found {len(failed_tests)} failed test(s). Here are the complete details:",
            border_style="red"
        ))
        
        for i, test in enumerate(failed_tests, 1):
            # Test header
            self.console.print(f"\n[bold red]═══ Failure {i}/{len(failed_tests)} ═══[/bold red]")
            
            # Test description
            description = test.get("description", "Unknown test")
            self.console.print(f"[bold cyan]Test:[/bold cyan] {description}")
            
            # All issues without truncation
            issues = test.get("issues", [])
            if issues:
                self.console.print(f"\n[bold red]Issues:[/bold red]")
                for issue in issues:
                    self.console.print(f"  • {issue}")
            
            # Expected vs actual comparison
            expected = test.get("expected", {})
            actual = test.get("actual_mcp_call", {})
            
            if expected or actual:
                comparison_table = Table(title="Expected vs Actual", box=box.ROUNDED, expand=True)
                comparison_table.add_column("Field", style="bold", width=15)
                comparison_table.add_column("Expected", style="green", min_width=20, overflow="fold")
                comparison_table.add_column("Actual", style="red", min_width=20, overflow="fold")
                
                # Compare tool
                comparison_table.add_row(
                    "Tool",
                    str(expected.get("tool", "N/A")),
                    str(actual.get("tool", "N/A"))
                )
                
                # Compare template
                comparison_table.add_row(
                    "Template", 
                    str(expected.get("template", "N/A")),
                    str(actual.get("template", "N/A"))
                )
                
                # Compare parameters
                expected_params = expected.get("parameters", [])
                actual_params = actual.get("parameters", [])
                comparison_table.add_row(
                    "Parameters",
                    "\n".join(str(p) for p in expected_params) if expected_params else "None",
                    "\n".join(str(p) for p in actual_params) if actual_params else "None"
                )
                
                self.console.print(comparison_table)
            
            # Separator between failures
            if i < len(failed_tests):
                self.console.print("\n" + "─" * 80)
    
    def _display_error(self, error_message: str) -> None:
        """Display error message in a panel"""
        error_panel = Panel(
            error_message,
            title="[bold red]Evaluation Error[/bold red]",
            border_style="red"
        )
        self.console.print(error_panel)
    
    def _get_pass_rate_color(self, pass_rate: str) -> str:
        """Get color for pass rate based on value"""
        try:
            rate = float(pass_rate.replace("%", ""))
            if rate >= 90:
                return "bright_green"
            elif rate >= 80:
                return "green"
            elif rate >= 70:
                return "yellow"
            elif rate >= 50:
                return "orange"
            else:
                return "red"
        except (ValueError, AttributeError):
            return "white"
    
    def _get_test_type(self, test_result: Dict[str, Any]) -> str:
        """Determine test type from description or result data"""
        description = test_result.get("description", "").lower()
        
        if "sequence" in test_result:
            return "Sequence"
        elif "gethubs" in description:
            return "GetHubs"
        elif "getprojects" in description:
            return "GetProjects"
        elif "getelementgroups" in description or "models" in description:
            return "Elements"
        elif "getnumberof" in description or "count" in description:
            return "Count"
        elif "filter" in description:
            return "Filter"
        elif "viewer" in description or "render" in description or "load" in description:
            return "Viewer"
        else:
            return "Other"
    
    def display_progress(self, current: int, total: int, description: str) -> None:
        """Display progress for ongoing evaluation"""
        percentage = (current / total) * 100 if total > 0 else 0
        
        progress_text = f"[cyan]Running test {current}/{total}[/cyan]: {description}"
        if len(progress_text) > 80:
            progress_text = progress_text[:77] + "..."
        
        self.console.print(progress_text)


def format_evaluation_results(results_json: str, json_output: bool = False) -> None:
    """
    Format evaluation results using Rich tables
    
    Args:
        results_json: JSON string containing evaluation results
        json_output: If True, also print raw JSON output
    """
    formatter = EvaluationResultsFormatter()
    
    # Display formatted results
    formatter.format_results(results_json)
    
    # Optionally display raw JSON
    if json_output:
        console = Console()
        console.print("\n" + "="*60)
        console.print("[bold]Raw JSON Output:[/bold]")
        console.print("="*60)
        
        try:
            results = json.loads(results_json)
            console.print_json(data=results)
        except json.JSONDecodeError:
            console.print("[red]Invalid JSON format[/red]")
            console.print(results_json)