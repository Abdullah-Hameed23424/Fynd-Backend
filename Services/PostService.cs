using Fynd.Api.Data;
using Fynd.Api.DTOs.FoundItem;
using Fynd.Api.DTOs.Home;
using Fynd.Api.DTOs.Posts;
using Fynd.Api.Models;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostResponse>> GetRecentPostsAsync()
        {
            var lostPosts = await _context.LostItems
            .Include(x => x.Category)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new PostResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Location = x.Location,
                Type = PostType.Lost,
                CreatedAt = x.CreatedAt,
                ImageUrl = x.ImageUrl,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Date = x.LostAt,
                UserId = x.UserId
            })
            .ToListAsync();

            var foundPosts = await _context.FoundItems
                .Include(x => x.Category)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new PostResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,
                    Type = PostType.Found,
                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    Date = x.FoundAt,
                    UserId = x.UserId
                })
                .ToListAsync();

            return lostPosts
                .Concat(foundPosts)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10);
        }

        public async Task<PagedPostResponse> GetPostsAsync(
        PostQueryParameters parameters)
        {
            var page = parameters.Page < 1
                ? 1
                : parameters.Page;

            var pageSize = parameters.PageSize < 1
                ? 10
                : Math.Min(parameters.PageSize, 50);

            // =========================
            // Lost Posts
            // =========================

            var lostPosts = _context.LostItems
                .AsNoTracking()
                .Select(x => new PostResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,

                    Type = PostType.Lost,

                    LostStatus = x.Status,
                    FoundStatus = null,

                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,

                    Date = x.LostAt,

                    UserId = x.UserId
                });

            // =========================
            // Found Posts
            // =========================

            var foundPosts = _context.FoundItems
                .AsNoTracking()
                .Select(x => new PostResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,

                    Type = PostType.Found,

                    LostStatus = null,
                    FoundStatus = x.Status,

                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,

                    Date = x.FoundAt,

                    UserId = x.UserId
                });

            // =========================
            // Search
            // =========================

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.Trim();

                lostPosts = lostPosts.Where(x =>
                    x.Title.Contains(search) ||
                    x.Description.Contains(search) ||
                    x.Location.Contains(search));

                foundPosts = foundPosts.Where(x =>
                    x.Title.Contains(search) ||
                    x.Description.Contains(search) ||
                    x.Location.Contains(search));
            }

            // =========================
            // Category Filter
            // =========================

            if (parameters.CategoryId.HasValue)
            {
                lostPosts = lostPosts.Where(x =>
                    x.CategoryId == parameters.CategoryId.Value);

                foundPosts = foundPosts.Where(x =>
                    x.CategoryId == parameters.CategoryId.Value);
            }

            // =========================
            // Location Filter
            // =========================

            if (!string.IsNullOrWhiteSpace(parameters.Location))
            {
                var location = parameters.Location.Trim();

                lostPosts = lostPosts.Where(x =>
                    x.Location.Contains(location));

                foundPosts = foundPosts.Where(x =>
                    x.Location.Contains(location));
            }

            // =========================
            // Lost Status Filter
            // =========================

            if (parameters.LostStatus.HasValue)
            {
                lostPosts = lostPosts.Where(x =>
                    x.LostStatus == parameters.LostStatus.Value);
            }

            // =========================
            // Found Status Filter
            // =========================

            if (parameters.FoundStatus.HasValue)
            {
                foundPosts = foundPosts.Where(x =>
                    x.FoundStatus == parameters.FoundStatus.Value);
            }

            // =========================
            // Type Filter
            // =========================

            IQueryable<PostResponse> posts;

            if (parameters.Type == PostType.Lost)
            {
                posts = lostPosts;
            }
            else if (parameters.Type == PostType.Found)
            {
                posts = foundPosts;
            }
            else
            {
                posts = lostPosts.Concat(foundPosts);
            }

            // =========================
            // Date Filter
            // =========================

            if (parameters.FromDate.HasValue)
            {
                posts = posts.Where(x =>
                    x.Date >= parameters.FromDate.Value);
            }

            if (parameters.ToDate.HasValue)
            {
                var toDate = parameters.ToDate.Value.Date.AddDays(1);

                posts = posts.Where(x =>
                    x.Date < toDate);
            }

            // =========================
            // Total Count
            // =========================

            var totalCount = await posts.CountAsync();

            // =========================
            // Sort
            // =========================

            posts = parameters.Sort switch
            {
                PostSort.Oldest =>
                    posts.OrderBy(x => x.CreatedAt),

                PostSort.Newest =>
                    posts.OrderByDescending(x => x.CreatedAt),

                _ =>
                    posts.OrderByDescending(x => x.CreatedAt)
            };

            // =========================
            // Pagination
            // =========================

            var pagedPosts = await posts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // =========================
            // Total Pages
            // =========================

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            // =========================
            // Response
            // =========================

            return new PagedPostResponse
            {
                Posts = pagedPosts,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<PostResponse?> GetPostDetailsAsync(
            PostDetailsQueryParameters parameters)
        {
            if (parameters.Type == PostType.Lost)
            {
                return await _context.LostItems
                    .AsNoTracking()
                    .Where(x => x.Id == parameters.Id)
                    .Select(x => new PostResponse
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Location = x.Location,

                        Type = PostType.Lost,

                        LostStatus = x.Status,
                        FoundStatus = null,

                        CreatedAt = x.CreatedAt,
                        ImageUrl = x.ImageUrl,

                        CategoryId = x.CategoryId,
                        CategoryName = x.Category.Name,

                        Date = x.LostAt,

                        UserId = x.UserId
                    })
                    .FirstOrDefaultAsync();
            }

            if (parameters.Type == PostType.Found)
            {
                return await _context.FoundItems
                    .AsNoTracking()
                    .Where(x => x.Id == parameters.Id)
                    .Select(x => new PostResponse
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Location = x.Location,

                        Type = PostType.Found,

                        LostStatus = null,
                        FoundStatus = x.Status,

                        CreatedAt = x.CreatedAt,
                        ImageUrl = x.ImageUrl,

                        CategoryId = x.CategoryId,
                        CategoryName = x.Category.Name,

                        Date = x.FoundAt,

                        UserId = x.UserId
                    })
                    .FirstOrDefaultAsync();
            }

            return null;
        }

        public async Task<PostResponse> CreatePostAsync(
            int userId,
            CreatePostRequest request)
        {
            if (request.Type == PostType.Found)
            {
                if (!request.FoundPriority.HasValue)
                    throw new ArgumentException("Found priority is required.");

                var item = new FoundItem
                {
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    Location = request.Location.Trim(),

                    FoundAt = request.Date,

                    Priority = request.FoundPriority.Value,

                    ImageUrl = request.ImageUrl?.Trim(),

                    UserId = userId,
                    CategoryId = request.CategoryId,

                    Status = FoundItemStatus.Found,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FoundItems.Add(item);

                await _context.SaveChangesAsync();

                await _context.Entry(item)
                    .Reference(x => x.Category)
                    .LoadAsync();

                return new PostResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Location = item.Location,

                    Type = PostType.Found,

                    LostStatus = null,
                    FoundStatus = item.Status,

                    Date = item.FoundAt,
                    CreatedAt = item.CreatedAt,

                    UserId = item.UserId,

                    ImageUrl = item.ImageUrl,

                    CategoryId = item.CategoryId,
                    CategoryName = item.Category.Name
                };
            }

            if (request.Type == PostType.Lost)
            {
                if (!request.LostPriority.HasValue)
                    throw new ArgumentException("Lost priority is required.");

                var item = new LostItem
                {
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    Location = request.Location.Trim(),

                    LostAt = request.Date,

                    Priority = request.LostPriority.Value,

                    ImageUrl = request.ImageUrl?.Trim(),

                    UserId = userId,
                    CategoryId = request.CategoryId,

                    Status = LostItemStatus.Lost,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LostItems.Add(item);

                await _context.SaveChangesAsync();

                await _context.Entry(item)
                    .Reference(x => x.Category)
                    .LoadAsync();

                return new PostResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Location = item.Location,

                    Type = PostType.Lost,

                    LostStatus = item.Status,
                    FoundStatus = null,

                    Date = item.LostAt,
                    CreatedAt = item.CreatedAt,

                    UserId = item.UserId,

                    ImageUrl = item.ImageUrl,

                    CategoryId = item.CategoryId,
                    CategoryName = item.Category.Name
                };
            }

            throw new ArgumentException("Invalid post type.");
        }

        public async Task<PagedPostResponse> GetMyPostsAsync(
            int userId,
            PostQueryParameters parameters)
        {
            var lostPosts = _context.LostItems
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new PostResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,

                    Type = PostType.Lost,

                    LostStatus = x.Status,
                    FoundStatus = null,

                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,

                    Date = x.LostAt,

                    UserId = x.UserId
                });

            var foundPosts = _context.FoundItems
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new PostResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,

                    Type = PostType.Found,

                    LostStatus = null,
                    FoundStatus = x.Status,

                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,

                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,

                    Date = x.FoundAt,

                    UserId = x.UserId
                });

            IQueryable<PostResponse> posts;

            if (parameters.Type == PostType.Lost)
            {
                posts = lostPosts;
            }
            else if (parameters.Type == PostType.Found)
            {
                posts = foundPosts;
            }
            else
            {
                posts = lostPosts.Concat(foundPosts);
            }

            var page = parameters.Page < 1
                ? 1
                : parameters.Page;

            var pageSize = parameters.PageSize < 1
                ? 10
                : Math.Min(parameters.PageSize, 50);

            var totalCount = await posts.CountAsync();

            posts = parameters.Sort switch
            {
                PostSort.Oldest =>
                    posts.OrderBy(x => x.CreatedAt),

                PostSort.Newest =>
                    posts.OrderByDescending(x => x.CreatedAt),

                _ =>
                    posts.OrderByDescending(x => x.CreatedAt)
            };

            var pagedPosts = await posts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            return new PagedPostResponse
            {
                Posts = pagedPosts,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<bool> UpdatePostAsync(
            int userId,
            int id,
            UpdatePostRequest request)
        {
            if (request.Type == PostType.Lost)
            {
                if (!request.LostPriority.HasValue)
                {
                    return false;
                }

                var item = await _context.LostItems
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.UserId == userId);

                if (item == null)
                {
                    return false;
                }

                item.Title = request.Title.Trim();
                item.Description = request.Description.Trim();
                item.Location = request.Location.Trim();
                item.LostAt = request.Date;
                item.Priority = request.LostPriority.Value;
                item.ImageUrl = request.ImageUrl?.Trim();
                item.CategoryId = request.CategoryId;

                await _context.SaveChangesAsync();

                return true;
            }

            if (request.Type == PostType.Found)
            {
                if (!request.FoundPriority.HasValue)
                {
                    return false;
                }

                var item = await _context.FoundItems
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.UserId == userId);

                if (item == null)
                {
                    return false;
                }

                item.Title = request.Title.Trim();
                item.Description = request.Description.Trim();
                item.Location = request.Location.Trim();
                item.FoundAt = request.Date;
                item.Priority = request.FoundPriority.Value;
                item.ImageUrl = request.ImageUrl?.Trim();
                item.CategoryId = request.CategoryId;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeletePostAsync(PostType postType, int userId, int id)
        {
            if(postType == PostType.Lost)
            {
                var item = await _context.LostItems
               .FirstOrDefaultAsync(x =>
                   x.Id == id &&
                   x.UserId == userId);

                if (item == null)
                {
                    return false;
                }

                _context.LostItems.Remove(item);

                await _context.SaveChangesAsync();

                return true;
            } else if(postType == PostType.Found)
            {
                var item = await _context.FoundItems
                  .FirstOrDefaultAsync(x =>
                      x.Id == id &&
                      x.UserId == userId);

                if (item == null)
                {
                    return false;
                }

                _context.FoundItems.Remove(item);

                await _context.SaveChangesAsync();

                return true;
            }
            return false;
        }
    }
}
