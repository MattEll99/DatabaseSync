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
    static Program()
    {
        sourceClient.BaseAddress = new Uri("http://localhost:5221/");
        sourceClient.DefaultRequestHeaders.Accept.Clear();
        sourceClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        targetClient.BaseAddress = new Uri("http://localhost:5220/");
        targetClient.DefaultRequestHeaders.Accept.Clear();
        targetClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
   
    static void Main()
    {
        // Console.WriteLine("Hello, World!");
        RunAsync().GetAwaiter().GetResult();
        
    }

  
    //Get Methods
    static async Task<SecurityPrinciple[]> GetSecurityPrinciples(string path, HttpClient client)
    {
        SecurityPrinciple[] securityPrinciples = new SecurityPrinciple[0];
        HttpResponseMessage response = await client.GetAsync(path);
        if (response.IsSuccessStatusCode)
        {
            securityPrinciples = await response.Content.ReadAsAsync<SecurityPrinciple[]>();
        }
        return securityPrinciples;
    }
    //Get all of the members from all of the groups.
    static async Task<GroupMember[]> GetAllGroupMembers(string path, HttpClient client)
    {
        GroupMember[] groupMembers = new GroupMember[0];
        HttpResponseMessage response = await client.GetAsync(path);
        if (response.IsSuccessStatusCode)
        {
            groupMembers = await response.Content.ReadAsAsync<GroupMember[]>();
        }
        return groupMembers;
    }
    static async Task<vGroupMembers[]> GetAllOfAGroupsMembers(string path, HttpClient client)
    {
        vGroupMembers[] groupMembers = new vGroupMembers[0];
        HttpResponseMessage response = await client.GetAsync(path);
        if (response.IsSuccessStatusCode)
        {
            groupMembers = await response.Content.ReadAsAsync<vGroupMembers[]>();
        }
        return groupMembers;
    }

    //Delete Methods.
    static async Task<string> DeleteSecurityPrincipleByDisplayName(string path, HttpClient client)
    {
        HttpResponseMessage response = await client.DeleteAsync(path);
        if (response.IsSuccessStatusCode)
        {
            return "success";
        }
        else
        {
            return "failed";
        }
    }
    static async Task<string> DeleteGroupMemberById(string path, HttpClient client)
    {
        HttpResponseMessage response = await client.DeleteAsync(path);
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
    static async Task<string> CreateSecurityPrinciple(string path, HttpClient client, HttpContent content)
    {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await client.PostAsync(path, content);
        if (response.IsSuccessStatusCode)
        {
            return "success";
        }
        else
        {
            return "failed";
        }
    }
    static async Task<string> CreateGroupMember(string path, HttpClient client)
    {
        HttpContent content = null;
        HttpResponseMessage response = await client.PostAsync(path, content);
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
            var sourcePrinciples = await GetSecurityPrinciples("api/SecurityPrinciple", sourceClient);
            var targetPrinciples = await GetSecurityPrinciples("api/SecurityPrinciple", targetClient);

            //Seperate source into Users, Groups and Service Principles.
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

            //Comparing sourceUsers with targetUsers.
            //Check if targetUser exists in sourceUsers, if not, delete it.
            foreach (SecurityPrinciple targetUser in targetUsers)
            {
                if (sourceUsersDisplayNames.Contains(targetUser.displayName))
                {
                    continue;
                }
                else
                {
                    await DeleteSecurityPrincipleByDisplayName($"api/SecurityPrinciple?displayName={targetUser.displayName}", targetClient);
                }
            }

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
                    await CreateSecurityPrinciple($"api/SecurityPrinciple/CreateSecurityPrinciple", targetClient, content);
                }
            }

            //Comparing sourceGroups with targetGroups.
            //Check if targetGroup exists in sourceGroup, if not, delete it.
            foreach (SecurityPrinciple targetGroup in targetGroups)
            {
                if (sourceGroupsDisplayNames.Contains(targetGroup.displayName))
                {
                    continue;
                }
                else
                {
                    await DeleteSecurityPrincipleByDisplayName($"api/SecurityPrinciple?displayName={targetGroup.displayName}", targetClient);
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
                    await CreateSecurityPrinciple($"api/SecurityPrinciple/CreateSecurityPrinciple", targetClient, content);
                }
            }


            //Comparing sourceServicePrinciples with targetServicePrinciples.
            //Check if targetServicePrinciple exists in sourceServicePrinciple, if not, delete it.
            foreach (SecurityPrinciple targetServicePrinciple in targetServicePrinciples)
            {
                if (sourceServicePrinciplesDisplayNames.Contains(targetServicePrinciple.displayName))
                {
                    continue;
                }
                else
                {
                    await DeleteSecurityPrincipleByDisplayName($"api/SecurityPrinciple?displayName={targetServicePrinciple.displayName}", targetClient);
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
                    await CreateSecurityPrinciple($"api/SecurityPrinciple/CreateSecurityPrinciple", targetClient, content);
                }
            }


            // Group tables.
            var sourceGroupMembers = await GetAllGroupMembers("api/GroupMember", sourceClient);
            var targetGroupmembers = await GetAllGroupMembers("api/GroupMember", targetClient);

            //Updated after partial sync.
            var sourcePrinciplesUpdated = await GetSecurityPrinciples("api/SecurityPrinciple", sourceClient);
            var targetPrinciplesUpdated = await GetSecurityPrinciples("api/SecurityPrinciple", targetClient);

            //Get members of each group from both source and target, then compare the lists.
            foreach (SecurityPrinciple group in sourceGroups)
            {
                var sourceVGroupMembers = await GetAllOfAGroupsMembers($"api/vGroupMember/GetVGroupMembersByGroupName?groupDisplayName={group.displayName}", sourceClient);
                var targetVGroupMembers = await GetAllOfAGroupsMembers($"api/vGroupMember/GetVGroupMembersByGroupName?groupDisplayName={group.displayName}", targetClient);

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
                        await DeleteGroupMemberById($"api/GroupMember/DeleteGroupMember?groupId={vgroupMember.groupId}&securityPrincipleId={vgroupMember.memberId}", targetClient);
                    }
                }

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
                       
                        await CreateGroupMember($"api/GroupMember/CreateGroupmember?groupId={groupPrinciple.Id}&securityPrincipleId={groupMemberToBeAdded.Id}", targetClient);
                    }
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
