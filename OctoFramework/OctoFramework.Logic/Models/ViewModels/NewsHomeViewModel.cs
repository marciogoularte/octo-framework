﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace OctoFramework.Logic.Models.ViewModels
{
    public partial class NewsHomeViewModel : MasterViewModel
    {
        public NewsHomeViewModel(IPublishedContent content) : base(content)
        {
        }

        public IEnumerable<NewsPostViewModel> NewsPosts { get { return Umbraco.TypedContentAtXPath("//blogPost").OrderByDescending(x => x.GetPropertyValue<DateTime>("createTime")).Select(x => new NewsPostViewModel(x)); } }
        public IEnumerable<TagModel> Tags { get { return Umbraco.TagQuery.GetAllContentTags().OrderByDescending(x => x.NodeCount); } }

        public IEnumerable<NewsPostViewModel> PagedBlogPosts(int page, string query, int pageSize = 10)
        {
            var posts = query == null ?
                NewsPosts
                : Umbraco.TagQuery.GetContentByTag(query).OrderByDescending(x => x.GetPropertyValue<DateTime>("createTime")).Select(x => new NewsPostViewModel(x));
            var resultsToSkip = page > 0 ? (page - 1) * pageSize : 0;
            var resultSet = posts.Skip(resultsToSkip)
                               .Take(pageSize);
            return resultSet;
        }
    }
}