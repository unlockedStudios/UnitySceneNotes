# UI Toolkit Styling Details

When working on UI Toolkit styling, reference the current version of the Unity Documentatmion for Style UI, and UI Toolkit.
- ℹ️ Always lookup the project version first, and verify that we are using that Unity versions USS Documentation.
- ℹ️ If a new component is needed, prioritize making it with a .uxml file and a .uss file. 
- ℹ️ Use csharp code as a last resort for styling issues. Unless I tell you to do it in csharp, or its a minor change.

## Clarification
This brief section is used to outline definitions, and intentions.
- What is a 'System'?
  - A system refers to all code and assets (UI, Controllers, services, etc.) that together implement a feature (e.g. inventory).
- Who is the 'developer'?
  - The developer is the team lead, or feature owner. In this case they are both the same person.

# Refactors
- Whenever refactoring occurs, always examine the entire system relevant to the feature, not just the UI.
For example, if you are working on the Inventory, review both the UI logic (e.g., VEInventoryController and its child UI components) and the core logic or services that support it (e.g., InventoryServiceComponent for database/cache).
This ensures that changes remain consistent and compatible across both the UI and the underlying systems that work together for that feature.
- Verify if a specific issue you're attempting to address, isn't already addressed in the .uss, or .uxml file.
- Check all direct and nested child components related to the system being refactored.
- Check if the logical flow from the UI parent, to the children make sense, and take into account the .uxml, and .uss files when making its decisions, and code.

# USS Styling and Rules.
- ✅ Whenever styling a USS file, double check the variables used, as we should be using variables, and values only available to Unitys UI Toolkit.
- Do not get styling request confused with Normal CSS styling, as it can break the stylings.
  - Do not use suffixes such as 'vw', and 'vh'
  - Do not use CSS stylings such as 'box-shadow'

# Creating Folders and Files
- ℹ️ When creating a new USS/UXML file, for a new System, always create a folder first, if it doesn't exist.
- ℹ️ Folders for UI Toolkit should be under ..UIToolkit/..
  - if its a stylesheet, it should be in ../StyleSheets/..
  - If its a uxml file, it should be under ../UIDocuments/..
- ℹ️ When a new .cs file that controls the .uxml is created, it should be prefixed with 'VE', and suffixed with 'controller'.
  - Example:
    - VEHealthBarController.cs
    - VENPCAlertController.cs
    - VESkillSystemController.cs
  - Names should be common and easily understood.
  - The maximum length of a name should be 35 characters.
  The .uss file, and .uxml file do not need the 'VE' prefix, as its already implied that they are UI files due to the file extensions.
- ℹ️ When a new UI System is made, it will usually be accompanied by a 'Controller' File. This is where the code for that UXML file will be executed.
  - For Example:
    - DamageNumbers.uss
    - DamageNumbers.uxml
    - UI/DmageNumbers/VEDamageNumberControllerExt.cs

## Depth in Files
- ℹ️ If a new Container-like box is needed, that serves as an independent feature, after an action exists, That independent feature should be created in a new set of Files. (.cs, .uss, .uxml)
- ℹ️ In most scenarios, the depth of the subfiles naming should not go further then 3.
  - If a UI system requires more than 3 levels of depth, refactor or group subcomponents to maintain clarity, OR consult with the developer for naming exceptions, or advice.
- ℹ️ If it is created in the context of an existing system, that new subcomponents file name should be pre-fixed with the previous System.
  - Example:
    - Previous System is called VESaveLoadController, and we need a Dialog Confirmation box when a person tries to hit the 'delete' button.
    - We need to create a dialogConfirm box. that DialogConfirm box should be called VESaveLoadDialogConfirm.
  - Example:
    - We have a system called VEInventoryController. When you hit a button, it extends a Stats Panel, that shows stat details of a character. When you hit that same button, it closes the stats panel.
    - The stats panel files should be named VEInventoryStatsPanel.
  - Example (Continued):
    - In the Inventory Stats panel, you want to create a Unique Label for some of the displayed stats. The Unique label(s) consist of the Header, and Entry Rows.
    - The Names would be VEInventoryStatsPanelEntry, and VEInventoryStatsPanelHeader.
    - Where the parent, VEInventroyStatsPanel, would be the owner.
- ℹ️ In all scenarios above, the 'Controller' also acts as the Manager of the sub systems it comes across. So the only one that Calls the subsystems, is the Controller.
  - Subcomponents should communicate with each other only via the parent controller, not directly.
  - This should help with loose coupling, simplify debugging, and keeping a system small, and digestable.
  - When a subcomponent needs to notify the parent of an event, it should raise an event or callback provided by the parent controller, rather then directly referencing other subcomponents.
- ℹ️ The names should be consise and short.
- ℹ️ If a subcomponent is shared, use a generic prefix, and place it in a shared folder with a neutral name.
  - Always use VEShared as the prefix for shared components. If a shared component becomes exclusive, rename, and relocate it.
  - Shared Scripts can be stored in ../02-Scripts/UI/Shared/..
  - Example:
    - Two System are called VEMainMenu, and VEInGameMainMenu.
    - In this scenario, they both have an 'exit Game' button, and in both scenarios it should exit the game.
    - A dialog box should popup, and state, "Are you sure?" with 2 buttons. Yes and No.
    - The name should be VESharedMenuExitGame.
      - Where 'exit game' is the suffix, VEShared is the prefix, and 'Menu' is the Shared convention between the two.
      - if there was no 'shared convention', simple call it, VESharedExitGame.
  

# Comments
- ℹ️ Prioritize commenting Groups of code in a pseudo-code like style. 
  - This is important for complex logic, and not required for trivial code.
- ℹ️ After creating the pseudocode, and comments, create the code to match.
- ❌ You do NOT need to leave comments when deleting code.
- ℹ️ Readability and Clarity in the pseudo-code should be a top priority. This code can be left in comments around the code.
  - Example:
    - In the VEInventoryStatsPanel, we need to create rows of entries, and pull the data from the database. The pseudocode could look like this:
    - The helper functions, like `CreateNewEntry(...)` does not need commenting, as the function name is self-explanatory.
```csharp
public void InitializeStatsPanel(List<ContractStats> stats)
{
  //for each stat...
  //verify we haven't already made an entry for this stat...
  if (myList.Any(x => x.name == stat.name)) return;

  //create a VEInventoryStatsPanelEntry..
  VEInventoryStatsPanelEntry entry = CreateNewEntry(stat);

  //Add the VEInventoryStatsPanel to an internal list maintained within the class.
  myList.Add(entry);
}

private VEInventoryStatsPanelEntry CreateNewEntry(ContractStats stat) { .... }
```