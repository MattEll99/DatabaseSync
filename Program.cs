// See https://aka.ms/new-console-template for more information
// See https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http;

public class SecurityPrinciple
{
    public int Id { get; set; }
    public required string applicationId { get; set; }
    public required string principleType { get; set; }
    public required string displayName { get; set; }
   
}

public class GroupMember
{
    public int groupId { get; set; }
    public int securityPrincipleId { get; set; }
}

public class vGroupMembers
{
    public int groupId { get; set; }
    public int memberId { get; set; }
    public string groupDisplayName { get; set; }
    public string memberDisplayName { get; set; }
    public string memberPrincipleType { get; set; }
}
class Program
{
    static HttpClient sourceClient = new HttpClient();
    static HttpClient targetClient = new HttpClient();
    static HttpClient httpClient = new HttpClient();
    static Program()
    {
        httpClient.BaseAddress = new Uri("http://localhost:5221/");
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
   
    static void Main()
    {
        RunAsync().GetAwaiter().GetResult();
    }

    //Get Methods
    static async Task<SecurityPrinciple[]> GetSecurityPrinciples(string path)
    {
        SecurityPrinciple[] securityPrinciples = new SecurityPrinciple[0];
        HttpResponseMessage response = await httpClient.GetAsync(path);
        if (response.IsSuccessStatusCode)
        {
            securityPrinciples = await response.Content.ReadAsAsync<SecurityPrinciple[]>();
        }
        return securityPrinciples;
    }

    //Get all of the members from all of the groups.
    static async Task<GroupMember[]> GetAllGroupMembers(string path)
    {
        GroupMember[] groupMembers = new GroupMember[0];
        HttpResponseMessage response = await httpClient.GetAsync(path);
        if (response.IsSuccessStatusCode)
        {
            groupMembers = await response.Content.ReadAsAsync<GroupMember[]>();
        }
        return groupMembers;
    }
    static async Task<vGroupMembers[]> GetAllOfAGroupsMembers(string path)
    {
        vGroupMembers[] groupMembers = new vGroupMembers[0];
        HttpResponseMessage response = await httpClient.GetAsync(path);
        if (response.IsSuccessStatusCode)
        {
            groupMembers = await response.Content.ReadAsAsync<vGroupMembers[]>();
        }
        return groupMembers;
    }

    //Delete Methods.

    //Havent tested this method!!!!!!!!!!!!!!!!!!!!!!!
    static async Task<string> DeleteSecurityPrincipleByDisplayName(string path)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync(path);
        if (response.IsSuccessStatusCode)
        {
            return "success";
        }
        else
        {
            return "failed";
        }
    }
    static async Task<string> DeleteGroupMemberById(string path)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync(path);
        if (response.IsSuccessStatusCode)
        {
            return "success";
        }
        else
        {
            return "failed";
        }
    }

    //Post Methods.

    //Untested!!!!!!!!!!!!!
    static async Task<string> CreateSecurityPrinciple(string path, HttpContent content)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await httpClient.PostAsync(path, content);
        if (response.IsSuccessStatusCode)
        {
            return "success";
        }
        else
        {
            return "failed";
        }
    }
    //Untested!!!!!!!!!!!!!!!!
    static async Task<string> CreateGroupMember(string path)
    {
        HttpContent content = null;
        HttpResponseMessage response = await httpClient.PostAsync(path, content);
        if (response.IsSuccessStatusCode)
        {
            return "success";
        }
        else
        {
            return "failed";
        }
    }

    static async Task RunAsync()
    {
        try
        {

            //Stores all the SecurityPrinciples from both Source and Target.
            var sourcePrinciples = await GetSecurityPrinciples("api/SecurityPrinciple/GetSecurityPrinciples?DbContext=S");
            var targetPrinciples = await GetSecurityPrinciples("api/SecurityPrinciple/GetSecurityPrinciples?DbContext=T");

            //Seperate into Users, Groups and Service Principles.
            var sourceUsers = sourcePrinciples.Where<SecurityPrinciple>(p => p.principleType == "user").ToList();
            var sourceGroups = sourcePrinciples.Where<SecurityPrinciple>(p => p.principleType == "Group").ToList();
            var sourceServicePrinciples = sourcePrinciples.Where<SecurityPrinciple>(p => p.principleType == "Service Principle").ToList();

            var targetUsers = targetPrinciples.Where<SecurityPrinciple>(p => p.principleType == "user").ToList();
            var targetGroups = targetPrinciples.Where<SecurityPrinciple>(p => p.principleType == "Group").ToList();
            var targetServicePrinciples = targetPrinciples.Where<SecurityPrinciple>(p => p.principleType == "Service Principle").ToList();

            //Converts to displayName as that is unique and common across both Dbs.
            var sourceUsersDisplayNames = sourceUsers.Select(p => p.displayName).ToList();
            var sourceGroupsDisplayNames = sourceGroups.Select(p => p.displayName).ToList();
            var sourceServicePrinciplesDisplayNames = sourceServicePrinciples.Select(p => p.displayName).ToList();

            var targetUsersDisplayNames = targetUsers.Select(p => p.displayName).ToList();
            var targetGroupsDisplayNames = targetGroups.Select(p => p.displayName).ToList();
            var targetServicePrinciplesDisplayNames = targetServicePrinciples.Select(p => p.displayName).ToList();

            // Security Principle Table Adds

            //Check if sourceUser exists in TargetUsers, if not, add it.
            foreach (SecurityPrinciple sourceUser in sourceUsers)
            {
                if (targetUsersDisplayNames.Contains(sourceUser.displayName))
                {
                    continue;
                }
                else
                {
                    sourceUser.Id = 0;
                    var content = JsonContent.Create<SecurityPrinciple>(sourceUser);
                    await CreateSecurityPrinciple($"api/SecurityPrinciple/CreateSecurityPrinciple?DbContext=T", content);
                }
            }
            //Check if sourceGroup exists in TargetGroups, if not, then add it.
            foreach (SecurityPrinciple sourceGroup in sourceGroups)
            {
                if (targetGroupsDisplayNames.Contains(sourceGroup.displayName))
                {
                    continue;
                }
                else
                {
                    sourceGroup.Id = 0;
                    var content = JsonContent.Create<SecurityPrinciple>(sourceGroup);
                    await CreateSecurityPrinciple($"api/SecurityPrinciple/CreateSecurityPrinciple?DbContext=T", content);
                }
            }
            //Check if sourceServicePrinciple exists in targetServicePrinciple, if not, then add it.
            foreach (SecurityPrinciple sourceServicePrinciple in sourceServicePrinciples)
            {
                if (targetServicePrinciplesDisplayNames.Contains(sourceServicePrinciple.displayName))
                {
                    continue;
                }
                else
                {
                    sourceServicePrinciple.Id = 0;
                    var content = JsonContent.Create<SecurityPrinciple>(sourceServicePrinciple);
                    await CreateSecurityPrinciple($"api/SecurityPrinciple/CreateSecurityPrinciple?DbContext=T", content);
                }
            }

            // Group tables.

            var sourceGroupMembers = await GetAllGroupMembers("api/GroupMember/GetAllGroupMembers?DbContext=S");
            var targetGroupmembers = await GetAllGroupMembers("api/GroupMember/GetAllGroupMembers?DbContext=T");

            //Updated after partial sync.
            var targetPrinciplesUpdated = await GetSecurityPrinciples("api/SecurityPrinciple/GetSecurityPrinciples?DbContext=T");
            var targetUsersUpdated = targetPrinciplesUpdated.Where<SecurityPrinciple>(p => p.principleType == "user").ToList();
            var targetGroupsUpdated = targetPrinciplesUpdated.Where<SecurityPrinciple>(p => p.principleType == "Group").ToList();
            var targetServicePrinciplesUpdated = targetPrinciplesUpdated.Where<SecurityPrinciple>(p => p.principleType == "Service Principle").ToList();

            //Get members of each group from both source and target, then compare them on displayName lists.
            foreach (SecurityPrinciple group in sourceGroups)
            {
                
                var sourceVGroupMembers = await GetAllOfAGroupsMembers($"api/vGroupMember/GetVGroupMembersByGroupName?groupDisplayName={group.displayName}&DbContext=S");
                var targetVGroupMembers = await GetAllOfAGroupsMembers($"api/vGroupMember/GetVGroupMembersByGroupName?groupDisplayName={group.displayName}&DbContext=T");

                var targetVGroupMembersDisplayNames = targetVGroupMembers.Select(p => p.memberDisplayName).ToList();
                var sourceVGroupMembersDisplayNames = sourceVGroupMembers.Select(p => p.memberDisplayName).ToList();

                //Deleting group members that aren't in source, but are in target.
                foreach (vGroupMembers vgroupMember in targetVGroupMembers)
                {
                    if (sourceVGroupMembersDisplayNames.Contains(vgroupMember.memberDisplayName))
                    {
                        continue;
                    }
                    else
                    {
                        await DeleteGroupMemberById($"api/GroupMember/DeleteGroupMember?groupId={vgroupMember.groupId}&securityPrincipleId={vgroupMember.memberId}&DbContext=T");
                    }
                }

                //Checks that all entries in TGroupMember table, have a corresponding SecurityPrinciple in the SP table. If not then delete.
                var targetGroupMembersList = await GetAllGroupMembers("api/GroupMember/GetAllGroupMembers?DbContext=T");
                var targetPrinciplesIdsUpdated = targetPrinciplesUpdated.Select(p => p.Id).ToList();
                foreach (GroupMember groupmember in targetGroupMembersList)
                {
                    if(targetPrinciplesIdsUpdated.Contains(groupmember.securityPrincipleId))
                    {
                        continue;
                    }
                    else
                    {
                        await DeleteGroupMemberById($"api/GroupMember/DeleteGroupMember?groupId={groupmember.groupId}&securityPrincipleId={groupmember.securityPrincipleId}&DbContext=T");
                    }

                }

                //Groupmember table

                //Adding group members that are in source, but aren't in target.
                foreach (vGroupMembers vgroup in sourceVGroupMembers)
                {
                    if (targetVGroupMembersDisplayNames.Contains(vgroup.memberDisplayName))
                    {
                        continue;
                    }
                    else
                    {
                        //Getting the UNIQUE displayName of the vgroup member to be added.
                        var XdisplayName = vgroup.memberDisplayName;
                        //Using the displayName to get the Principle in Target Db. (This is to get the Id from the correct Db.)
                        var groupMemberToBeAdded = targetPrinciplesUpdated.Where(x => x.displayName == XdisplayName).FirstOrDefault();

                        //Getting the UNIQUE displayName of the vgroup Group to be added.
                        var XgroupName = vgroup.groupDisplayName;
                        //Using the displayName to get the Principle in Target Db. (This is to get the Id from the correct Db.)
                        var groupPrinciple = targetPrinciplesUpdated.Where(x => x.displayName == XgroupName).FirstOrDefault();

                        await CreateGroupMember($"api/GroupMember/CreateGroupMember?groupId={groupPrinciple.Id}&securityPrincipleId={groupMemberToBeAdded.Id}&DbContext=T");
                    }
                }
            }

            //Deleting Security Principles

            //Check if targetUser exists in sourceUsers, if not, delete it.
            foreach (SecurityPrinciple targetUser in targetUsers)
            {
                if (sourceUsersDisplayNames.Contains(targetUser.displayName))
                {
                    continue;
                }
                else
                {
                    await DeleteSecurityPrincipleByDisplayName($"api/SecurityPrinciple/DeleteSecurityPrincipleByDisplayName?displayName={targetUser.displayName}&DbContext=T");
                }
            }
            //Check if targetGroup exists in sourceGroup, if not, delete it.
            foreach (SecurityPrinciple targetGroup in targetGroups)
            {
                if (sourceGroupsDisplayNames.Contains(targetGroup.displayName))
                {
                    continue;
                }
                else
                {
                    await DeleteSecurityPrincipleByDisplayName($"api/SecurityPrinciple/DeleteSecurityPrincipleByDisplayName?displayName={targetGroup.displayName}&DbContext=T");
                }
            }
            //Check if targetServicePrinciple exists in sourceServicePrinciple, if not, delete it.
            foreach (SecurityPrinciple targetServicePrinciple in targetServicePrinciples)
            {
                if (sourceServicePrinciplesDisplayNames.Contains(targetServicePrinciple.displayName))
                {
                    continue;
                }
                else
                {
                    await DeleteSecurityPrincipleByDisplayName($"api/SecurityPrinciple/DeleteSecurityPrincipleByDisplayName?displayName={targetServicePrinciple.displayName}&DbContext=T");
                }
            }

            


          
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.ReadLine();
    }
}
