The entry-point for Autodesk entity queries. This acts as the top-level API from which all queries must start.

elementGroupAtTip(elementGroupId: ID!): ElementGroup
Retrieves latest elementGroup data based on given ID.
@param {ID} elementGroupId - The ID of the elementGroup.

elementGroupByVersionNumber(
elementGroupId: ID!
versionNumber: Int!
): ElementGroup
Retrieves elementGroup by version number and ID.
@param {Int} versionNumber - Version number to be retrieved.
@param {ID} elementGroupId - The ID of the elementGroup.

elementGroupsByHub(
hubId: ID!
filter: ElementGroupFilterInput
pagination: PaginationInput
): ElementGroups!
Retrieves elementGroups in the given hub, using additional RSQL filters if provided.
@param {ID} hubId - Hub to retrieve elementGroups from.
@param {ElementGroupFilterInput=} filter - RSQL filter to use for searching elementGroups.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementGroupsByProject(
projectId: ID!
filter: ElementGroupFilterInput
pagination: PaginationInput
): ElementGroups!
Retrieves elementGroups in the given project, using additional RSQL filters if provided.
@param {ID} projectId - Project to retrieve elementGroups from.
@param {ElementGroupFilterInput=} filter - RSQL filter to use for searching elementGroups.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementGroupsByFolder(
projectId: ID!
folderId: ID!
filter: ElementGroupFilterInput
pagination: PaginationInput
): ElementGroups!
Retrieves elementGroups in the given folder, using additional RSQL filters if provided.
@param {ID} projectId - Project to retrieve elementGroups from.
@param {ID} folderId - Folder to retrieve elementGroups from.
@param {ElementGroupFilterInput=} filter - RSQL filter to use for searching elementGroups.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementGroupsByFolderAndSubFolders(
projectId: ID!
folderId: ID!
filter: ElementGroupFilterInput
pagination: PaginationInput
): ElementGroups!
Retrieves elementGroups in the given folder and it's sub-folders recursively, using additional RSQL filters if provided.
@param {ID} projectId - Project to retrieve elementGroups from.
@param {ID} folderId - Folder to recursively retrieve elementGroups from.
@param {ElementGroupFilterInput=} filter - RSQL filter to use for searching elementGroups.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementGroupExtractionStatus(
fileUrn: ID!
versionNumber: Int = 1
): ElementGroupExtractionStatus
Retrieves the extraction status of the given elementGroup.
@param {ID} fileUrn - File to retrieve elementGroup extraction status from.
@param {Int} versionNumber - File version to retrieve elementGroup extraction status from. Default value is 1.

elementGroupExtractionStatusAtTip(
fileUrn: ID!
accProjectId: ID!
): ElementGroupExtractionStatus
Retrieves the extraction status of the given elementGroup for the latest version.
@param {ID} fileUrn - File to retrieve elementGroup extraction status from.
@param {Int} accProjectId - ACC Project Id of the elementGroup.

elementAtTip(elementId: ID!): Element
Retrieves element using given ID.
@param {ID} elementId - Element to retrieve.

elementsByHub(
hubId: ID!
filter: ElementFilterInput
pagination: PaginationInput
): Elements
Retrieves elements from given hub, using additional RSQL filters if provided.
@param {ID} hubId - Hub to retrieve elements from.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementsByProject(
projectId: ID!
filter: ElementFilterInput
pagination: PaginationInput
): Elements
Retrieves elements from given project, using additional RSQL filters if provided.
@param {ID} projectId - Project to retrieve elements from.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementsByFolder(
projectId: ID!
folderId: ID!
filter: ElementFilterInput
pagination: PaginationInput
): Elements
Retrieves elements from given folder, using additional RSQL filters if provided.
@param {ID} projectId - Project to retrieve elements from.
@param {ID} folderId - Folder to retrieve elements from.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementsByElementGroup(
elementGroupId: ID!
filter: ElementFilterInput
pagination: PaginationInput
): Elements
Retrieves elements from given elementGroup, using additional RSQL filters if provided.
@param {ID} elementGroupId - ElementGroup to retrieve elements from.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementsByElementGroups(
elementGroupIds: [ID!]!
filter: ElementFilterInput
pagination: PaginationInput
): Elements
Retrieves elements from a given set of elementGroups, using additional RSQL filters if provided.
@param {[ID]} elementGroupIds - ElementGroups to retrieve elements from. Up to 25 elementGroup IDs can be provided.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

elementsByElementGroupAtVersion(
elementGroupId: ID!
versionNumber: Int!
filter: ElementFilterInput
pagination: PaginationInput
): Elements
Retrieves elements from given elementGroup at given elementGroup version, using additional RSQL filters if provided.
@param {ID} elementGroupId - ElementGroup to retrieve elements from.
@param {Int} versionNumber - ElementGroup version to retrieve elements from.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

distinctPropertyValuesInElementGroupById(
elementGroupId: ID!
propertyDefinitionId: ID!
filter: ElementFilterInput
): DistinctPropertyValues
Retrieves distinct values in an ElementGroup given a property definition ID.
@param {ID} elementGroupId - ElementGroup to retrieve distinct values from.
@param {ID} propertyDefinitionId - definition id of the property to retrieve the distinct values of.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.

distinctPropertyValuesInElementGroupByName(
elementGroupId: ID!
name: String!
filter: ElementFilterInput
pagination: PaginationInput
): DistinctPropertyValuesCollection
Retrieves distinct values in an ElementGroup given a property name.
@param {ID} elementGroupId - ElementGroup to retrieve distinct values from.
@param {String} name - name of the property to retrieve the distinct values of.
@param {ElementFilterInput=} filter - RSQL filter to use for searching elements.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

propertyDefinitionsByElementGroup(
elementGroupId: ID!
filter: PropertyDefinitionFilterInput
pagination: PaginationInput
): PropertyDefinitions!
Get all Property Definitions used in specified elementGroup
@param {ID} elementGroupId - ElementGroup to retrieve property definitions of.
@param {PropertyDefinitionFilterInput=} filter - Specifies how to filter on property definitions.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

hub(hubId: ID!): Hub
Retrieves an object representing a hub.

hubByDataManagementAPIId(dataManagementAPIHubId: ID!): Hub
Retrieves an object representing a hub by its external id.

hubs(
filter: HubFilterInput
pagination: PaginationInput
): Hubs
Retrieves all hubs that match the specified criteria.

project(projectId: ID!): Project
Retrieves an object representing a project from a specified hub.

projectByDataManagementAPIId(dataManagementAPIProjectId: ID!): Project
Retrieves an object representing a project by its external id.

projects(
hubId: ID!
filter: ProjectFilterInput
pagination: PaginationInput
): Projects
Retrieves all projects that match the specified filter criteria from a specified hub.
@param {ID} hubId - The ID of the hub that contains the projects.
@param {ProjectFilterInput=} filter - Specifies how to filter a list of projects. You can filter by name.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

folder(
projectId: ID!
folderId: ID!
): Folder
Retrieve folder specified by the provided Id.
@param {ID} projectId - The ID of the project that contains the item.
@param {ID} folderId - The ID of the item to retrieve.

foldersByFolder(
projectId: ID!
folderId: ID!
filter: FolderFilterInput
pagination: PaginationInput
): Folders
Retrieves all subfolders within a specified folder that meet the filter criteria specified by the filter argument.
@param {ID} projectId - The ID of the project that contains the items.
@param {ID} folderId - The ID of the folder that contains the subfolders.
@param {FolderFilterInput=} filter - Specifies how to filter on folders. You can filter by name.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.

foldersByProject(
projectId: ID!
filter: FolderFilterInput
pagination: PaginationInput
): Folders
Retrieves all top level folders under a specified project that meet the filter criteria specified by the filter argument.
@param {ID} projectId - The ID of the project that contains the items.
@param {FolderFilterInput=} filter - Specifies how to filter on folders. You can filter by name.
@param {PaginationInput=} pagination - Specifies how to split the response into multiple pages.