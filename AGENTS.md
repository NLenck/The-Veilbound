# Development workflow

- Make code changes on a feature or fix branch, with focused commits pushed to GitHub.
- Keep main as the shared integration branch; open a pull request for completed work.
- Keep Unity .meta files with their assets. Exclude builds, caches, credentials and local archives from commits.
- Put new equipment rules in focused domain classes. UI should consume definitions and calculated results rather than duplicate gameplay formulas.
- Armour type set bonuses and item enchantments are separate systems. Validate their effects independently and together.
- Preserve character save compatibility and existing item identities when refactoring serialized models.
- Run checks appropriate to each change and record meaningful validation in the pull request.
