﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Phoebe.Analytics;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Reactive;
using XPlatUtils;
using Toggl.Phoebe.Helpers;

namespace Toggl.Phoebe.ViewModels
{
    public interface IOnTagSelectedHandler
    {
        void OnCreateNewTag (ITagData newTagData);

        void OnModifyTagList (List<ITagData> newTagList);
    }

    public class TagListVM : IDisposable
    {
        // This viewMode is apparently simple but
        // it needs the code related with the update of
        // the list. (subscription and reload of data
        // everytime a tag is changed/created/deleted

        public TagListVM (AppState appState, Guid workspaceId, List<Guid> previousSelectedIds)
        {
            TagCollection = LoadTags (appState, workspaceId, previousSelectedIds);
            ServiceContainer.Resolve<ITracker> ().CurrentScreen = "Select Tags";
        }

        public void Dispose ()
        {
            TagCollection = null;
        }

        public ObservableRangeCollection<ITagData> TagCollection { get; set; }

        private ObservableRangeCollection<ITagData> LoadTags (
            AppState appState, Guid workspaceId, List<Guid> previousSelectedIds)
        {
            var tagCollection = new ObservableRangeCollection<ITagData> ();
            var workspaceTags = appState.Tags.Values
                                .Where (r => r.DeletedAt == null && r.WorkspaceId == workspaceId);
            tagCollection.AddRange (workspaceTags);
            return tagCollection;
        }
    }
}
