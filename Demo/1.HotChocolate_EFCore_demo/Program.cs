using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);
var DbPath = System.IO.Path.Join(path, "blogging.db");

builder.Services.AddPooledDbContextFactory<BloggingContext>
    (b => b.UseSqlite($"Data Source={DbPath}"));
builder.Services.AddDbContextPool<BloggingContext>(b => b.UseSqlite($"Data Source={DbPath}"));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Queries>()
    .AddMutationType<Mutations>()
    //Add Dependency injections for various facilities.
    .AddSorting()
    .AddProjections()
    .AddFiltering()
    .AddMutationConventions(applyToAllMutations: true) //Automatically use all conventions for GraphQL/Relay, for consistency.
    .AddQueryableCursorPagingProvider();

var app = builder.Build();
app.MapGraphQL();
app.MapGet("/", () => Results.LocalRedirect("/graphql"));

app.Run();


public class Queries
{
    [UseDbContext(typeof(BloggingContext)), UsePaging(MaxPageSize = 100, IncludeTotalCount = true), UseProjection, UseFiltering, UseSorting]
    public IQueryable<Blog>? Blogs([ScopedService] BloggingContext db) => db.Blogs;

    [UseDbContext(typeof(BloggingContext)), UsePaging(MaxPageSize = 100, IncludeTotalCount = true), UseProjection, UseFiltering, UseSorting]
    public IQueryable<Post>? Posts([ScopedService] BloggingContext db) => db.Posts;
}




public class Mutations
{
    public async Task<Blog?> CreateBlog([Service] BloggingContext dbContext, string url)
    {
        int newId = dbContext.Blogs.AsEnumerable().Select(b => b.BlogId).DefaultIfEmpty(0).Max(i => i + 1);
        var blog = new Blog() { BlogId = newId, Url = url };
        dbContext.Add(blog);
        dbContext.SaveChanges();
        return blog;
    }

    [Error(typeof(InvalidBlogIdException))]
    public async Task<Post?> CreatePost([Service] BloggingContext dbContext, Post post)
    {
        int newId = dbContext.Posts.AsEnumerable().Select(p => p.PostId).DefaultIfEmpty(0).Max(i => i + 1);
        post.PostId = newId;

        if (!dbContext.Blogs.Any(b => b.BlogId == post.BlogId))
            throw new InvalidBlogIdException(post.BlogId);

        dbContext.Add(post);
        dbContext.SaveChanges();
        return post;
    }
}

public class InvalidBlogIdException : Exception
{
    public InvalidBlogIdException(int blogId) : base($"The blog with Id {blogId} does not exist.") { }
}
