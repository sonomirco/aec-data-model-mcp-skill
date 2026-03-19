# reference library for extension bundles, skills, and mcp

This is the single consolidated reference file for the presentation package.

## file naming convention

Use lowercase kebab-case for files in `docs/presentation`.

## core framing notes

- The presentation should emphasize a full extension bundle, not mcp alone.
- The bundle framing is: skills + mcp server + app/ui layer.
- For the AEC demo flow, use mcp apps after data retrieval to render charts, model highlights, and interactive outputs.
- Keep the message practical: no rip-and-replace, wrap existing systems and data.

## platform and packaging references

- MCP apps (official): https://blog.modelcontextprotocol.io/posts/2026-01-26-mcp-apps/
  Notes: tools can return interactive UI in-chat; production-ready with sandboxing and consent model.
- Gemini CLI extensions guide: https://geminicli.com/docs/extensions/writing-extensions/
  Notes: extension package can include mcp servers, commands, skills, context files, hooks, and themes.
- Claude skills explained: https://claude.com/blog/skills-explained
  Notes: positions skills with prompts, projects, subagents, and mcp as complementary building blocks.
- AECDM Claude skill reference: https://github.com/asertorio/aecdm-claude-skill
  Notes: concrete AEC example with Autodesk auth, GraphQL query execution, and viewer integration.
- MCPs, CLIs, and skills (when to use what): https://jngiam.bearblog.dev/mcps-clis-and-skills-when-to-use-what/
  Notes: practical decision boundaries between interface styles.

## tradeoff and architecture references

- CLI vs MCP: https://kanyilmaz.me/2026/02/23/cli-vs-mcp.html?utm_source=tldrdev
  Notes: argues CLI wrappers can reduce context/token overhead versus full MCP schema exposure in some workflows.
- Skills vs MCP tools (LlamaIndex): https://www.llamaindex.ai/blog/skills-vs-mcp-tools-for-agents-when-to-use-what
  Notes: mcp for deterministic external operations and shared source of truth; skills for lightweight domain behavior.
- Context management and MCP: https://cra.mr/context-management-and-mcp/?utm_source=tldrai
  Notes: frames context-rot as the bigger issue and recommends isolation patterns (for example, subagents).

## eval and testing references

- OpenAI eval skills: https://developers.openai.com/blog/eval-skills
  Notes: patterns for creating reusable evaluation logic and operational eval workflows.
- Testing skills (Phil Schmid): https://www.philschmid.de/testing-skills
  Notes: practical test strategies for reliability and regression control in skill behavior.

## research references and takeaways

- SkillsBench: https://arxiv.org/html/2602.12670v1#S1
  Key takeaways:
  - Human-curated skills improved average performance (+16.2 points) while self-generated skills showed little value.
  - Small focused skill modules outperform broad documentation dumps.
  - Skill impact is domain-dependent and must be benchmarked per use case.
- The molecular structure of thought: https://arxiv.org/html/2601.06002v2
  Key takeaways:
  - Long reasoning is structural, not just linear chain length.
  - Quality depends on maintaining coherent reasoning topology over many steps.
- Evaluating AGENTS.md context files: https://arxiv.org/html/2602.11988v1
  Supporting article: https://www.humanlayer.dev/blog/writing-a-good-claude-md
  Key takeaways:
  - Overloaded or auto-generated context files can reduce success and increase cost.
  - Minimal, high-signal, human-authored context works better than broad instruction noise.
- Agent skills for LLMs (architecture, acquisition, security): https://arxiv.org/html/2602.12430v3
  Key takeaways:
  - Strong skill bundles include instructions, templates/scripts, and verification steps.
  - Security should be capability-based so skills only access declared tools.
  - Portability between model ecosystems remains a major challenge.

## companion presentation file

- presentation narrative and audience arc: `docs/presentation/presentation-outline.md`
