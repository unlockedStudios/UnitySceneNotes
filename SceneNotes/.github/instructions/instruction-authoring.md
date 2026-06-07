# Instruction Authoring

Use this file when creating or updating repository instruction files, style-guide sections, formatting rules,
example patterns, or guidance about how instructions should be written.

Future requests about instruction formatting, section layout, summary/detail style, Dos and Don'ts formatting, or
documentation guidance belong here unless they are specific to a subsystem file.

## Section Formatting

New guidance should have one clear section heading, followed by the short details needed to understand the rule.
When a section needs positive and negative examples, place them under Dos and Don'ts headings instead of repeating
the section name in each heading.

### Dos

- Do use one descriptive section heading, then explain the rule directly under it.
- Do use `Dos` and `Don'ts` headings under that section when the guidance needs clear positive and negative rules.
- Do match the heading depth of the surrounding document. For example, use `### Dos` under a `##` section.
- Do keep examples close to the rule they explain.

### Don'ts

- Don't create headings like `Inspector Validation Dos` and `Inspector Validation Don'ts` under an
  `Inspector Validation` section.
- Don't add a new top-level instruction file for every small style preference.
- Don't bury format rules in subsystem files unless the rule only applies to that subsystem.

## Summaries And Details

Instruction summaries should tell future agents what the rule is for and when to apply it. Details should be short,
specific, and actionable enough that another agent can follow the rule without guessing.

### Dos

- Do lead with the intended behavior, then add examples only where they prevent ambiguity.
- Do state when the instruction file should be loaded if the route is not obvious.
- Do keep lists short and focused on decisions future agents actually need to make.
- Do name the preferred default when there are multiple acceptable options.

### Don'ts

- Don't restate the same rule in several nearby bullets.
- Don't add broad policy language that could accidentally apply to unrelated systems.
- Don't include long implementation inventories unless they prevent a likely mistake.
