# Surfing unstructured data in construction: a new era of accessibility

---

## 1. Outcome

By the end of the presentation, the audience will decide to expose their existing data — APIs and documentation — through an AI-native access layer. They will walk away with a concrete mental model of an Extension Bundle (Skill + MCP + MCPApp) as the new standard for making construction data consumable, and a clear first step they can take without replacing any existing system.

---

## 2. Why should they care? (Relevance)

Construction is the world's most data-rich but data-starved sector. We have been building files — RVTs, PDFs, IFCs, contracts, specs — for 30 years, yet poor interoperability costs the US industry $16 billion annually. 90% of project data goes unused. Teams spend up to 60% of their workday wrestling with information that exists but cannot be reached.

The problem is not a lack of data. It is the fragmentation of the data — siloed across tools, formats, and teams — and the fact that none of it is readable by the agents that could actually help us consume it.

AI's biggest economic payoff is not automation — it is coordination. The real cost in our industry is not execution; it is translation — the friction required to make architects, structural engineers, and contractors interpret each other across incompatible systems. When a stairwell moves, the cost is not the redesign. It is the manual re-coordination across every tool, team, and format that follows. AI reduces that translation cost by making it cheap to extract structure from unstructured information and act across incompatible systems — without forcing anyone to change their tools. The answer is not a new platform. It is a hidden coordination layer built on well-documented APIs, domain expertise encoded in Skills, and MCP servers that make your data reachable by any agent.

Consider what happened in software: Resend became the default email API recommendation across millions of AI interactions, not because it was the best tool, but because it structured its documentation so agents could read it. Groq, despite being faster and cheaper, kept losing because its docs were harder to parse. The thing that gets chosen is not the best product. It is the most readable one.

We face the same choice. The winners in AEC will not be those with the best systems. They will be those who become the coordination layer — the most agent-ready data interface in the room.

---

## 3. Know → Feel

### Transition 1: from storage to interface

Know — your data problem is an interface problem. Decades of project knowledge are buried in unstructured PDFs, emails, BIM models, and siloed platforms. The bottleneck is not volume. It is the last mile: making all of it queryable in natural language.

Feel — recognition. The audience should feel that you have finally named the invisible wall they hit every day. This is not a new data problem. It is a new access problem.

### Transition 2: the power of the Extension Bundle

Know — you do not need to rebuild your systems. You need to wrap them in a composable Extension. This bundle includes three components that work together:

- Skills: human-curated procedural knowledge that teaches the agent how to navigate your specific domain. Research shows a 16.2% performance improvement over generic prompting. Skills are the playbook.
- MCP servers: a standardized, deterministic connection to your external tools and APIs — handling authentication, permissions, and structured data retrieval. MCP is the data pipeline.
- MCPApps: real-time interactive UI that renders directly in the conversation — folder structures, element selection, 3D visualization, dashboards. MCPApps is what the user sees.

The AEC Data Model GraphQL API is the live backbone. Wrap it with a Skill that teaches the agent to construct queries, an MCP server that handles Autodesk authentication, and MCPApp that renders elements in 3D. No new data. No new platforms. No rip-and-replace.

Feel — possibility. "We can do this with what we already have."

### Transition 3: the race for the standard

Know — this is not theoretical. The AECDM Claude Skill on GitHub is a working reference implementation today: natural language queries against architectural models, GraphQL discovery, 3D visualization rendered directly in the conversation. Someone already built it. The question is not whether it can be done. It is who in the AEC industry will own how it gets packaged and distributed.

Feel — urgency. If we do not define how construction data is bundled for agents, someone outside the industry will define it for us.

---

## 4. The overall feel: from overwhelmed to agency

The emotional arc moves the room from the heavy weight of fragmented data to the lightness of surfing it. The shift is an identity shift — from File Managers to Interface Builders. The files are not going away. We are finally making them speak.

---

## 5. Main point

"Stop wrestling with your data. Start surfing it."

Or: "You are not competing against bad data. You are competing against someone else's readable data."

The frame is the bundle. MCP alone, Skills alone, or GraphQL alone is not the story. The story is the composable Extension that wraps the AEC Data Model and makes decades of construction data consumable through a conversation. That is the demo, the concept, and the standard worth defining.

---

## 6. The story: Sara surfs

Sara is a project architect on a mid-rise residential tower. She is three weeks from permit submission. The building is in Revit. The specifications are in PDFs. The compliance references are in a folder of Word documents.

Then the structural engineer sends a message: the stairwell on Level 4 has moved 400mm to comply with the revised egress path.

Sara knows what this means. Not the redesign — the redesign takes an hour. What it means is the four hours that follow. Four hours of opening schedules, cross-referencing door tags, checking corridor widths, tracking down the fire rating table buried on page 47 of the spec, downloading the relevant code section on egress, and manually verifying that every downstream element touching that stairwell still complies. Four hours of wrestling. Four hours of her workday spent not designing.

Today, she does something different.

She types into the conversation window: "Show me every door on Level 4 within 5 metres of stairwell 4A. Include the fire rating, door width, and the room it serves."

The agent reaches into the AEC Data Model through the MCP server. It extracts element-level geometry — door positions, widths, associated room boundaries — without Sara opening Revit or touching a schedule. It locates each element by room automatically, using the spatial relationships embedded in the model. The results appear directly in the conversation. Seven doors. Two of them now fall inside the revised setback zone. The MCPApp renders them highlighted in the 3D model, right there in the chat. Sara can see exactly where the conflict lives.

She then asks: "Do these doors meet the door width requirements in our project specification?"

The agent takes the extracted geometry and compares it against the written project spec — a PDF it has already been given access to through the same MCP layer. No schema knowledge required. No manual cross-referencing. The Skill knows how to read both the structured model data and the unstructured specification text, and it returns the comparison directly: door D4-09, specified minimum 820mm clear opening, model shows 810mm. One flag. One source of truth pulled from two previously incompatible formats.

Sara then asks one more question: "Is that width also compliant with the NCC egress requirements for a residential corridor?"

Now the agent moves to a second service. The building code MCP — connected to Archie, an AI assistant trained on the National Construction Code — receives the same door data and cross-references it against the regulatory minimums. The answer returns with the specific clause number and the threshold. The project spec and the building code agree: door D4-09 needs to change. One conversation. Two compliance checks. Zero PDFs opened manually.

Sara did not know which section of the code to search. She did not know the spec was in a different format from the model. The agent translated her intent across three sources — the BIM geometry, the written specification, and the regulatory code — and returned one structured answer. That is the translation cost that AI makes cheap.

"Give me a bar chart of all fire-rated doors on this floor, grouped by rating."

The MCPApp generates the chart inline. Three rating categories. Nine doors. Two below the threshold for the revised fire compartment. Sara forwards the chart directly to the fire engineer. No spreadsheet. No manual count. The data was already in the model — it just finally had a way to speak.

The whole interaction takes eleven minutes.

Sara did not install new software. She did not change her Revit workflow. She did not ask IT for a new platform. She used the data she already had. The Skill taught the agent how to navigate AEC Data Model queries. The APS MCP server handled Autodesk authentication without her touching a credential. The building code MCP brought the code knowledge into the same conversation — not as a separate tab or a separate search, but as a service the agent called on her behalf. The MCPApp made everything visible.

Sara does not think about any of this. She thinks: "This is how it should have always worked."

That feeling — that specific recognition — is the identity shift this presentation is trying to create. Sara is no longer a File Manager. She is an Interface Builder. The files did not change. The model did not change. What changed is that the data finally became surfable.
