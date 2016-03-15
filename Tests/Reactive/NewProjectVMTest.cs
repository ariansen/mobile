﻿using System;
using System.Linq;
using NUnit.Framework;
using Toggl.Phoebe._Data;
using Toggl.Phoebe._Data.Models;
using Toggl.Phoebe._Net;
using Toggl.Phoebe._Reactive;
using Toggl.Phoebe._ViewModels;
using XPlatUtils;
using Toggl.Phoebe.Analytics;
using System.Threading.Tasks;

namespace Toggl.Phoebe.Tests.Reactive
{
    [TestFixture]
    public class NewProjectVMTest : Test
    {
        NewProjectVM viewModel;
        SyncSqliteDataStore dataStore;
        readonly ToggleClientMock togglClient = new ToggleClientMock ();

        public override void Init ()
        {
            base.Init ();

            var initState = Util.GetInitAppState ();
            var platformUtils = new PlatformUtils ();
            ServiceContainer.RegisterScoped<IPlatformUtils> (platformUtils);
            ServiceContainer.RegisterScoped<ITogglClient> (togglClient);
            ServiceContainer.RegisterScoped<ITracker> (new TrackerMock());

            RxChain.Init (initState);
            viewModel = new NewProjectVM (initState.TimerState, Util.WorkspaceId);
            dataStore = new SyncSqliteDataStore (databasePath, platformUtils.SQLiteInfo);
        }

        public override void Cleanup ()
        {
            base.Cleanup ();
            RxChain.Cleanup ();
        }

        [Test]
        public async Task TestSaveProject ()
        {
            var pcolor = 2;
            var pname = "MyProject";
            var tcs = Util.CreateTask<bool> ();

            viewModel.SaveProject (pname, pcolor, new SyncTestOptions (false, (state, sent, queued) => {
                try {
                    ProjectData project = null;
                    Assert.That (project = state.TimerState.Projects.Values.SingleOrDefault (
                                               x => x.WorkspaceId == Util.WorkspaceId && x.Name == pname && x.Color == pcolor), Is.Not.Null);

                    // Check project has been correctly saved in database
                    Assert.That (dataStore.Table<ProjectData> ().SingleOrDefault (
                                     x => x.WorkspaceId == Util.WorkspaceId && x.Name == pname && x.Color == pcolor && x.Id == project.Id), Is.Not.Null);

                    // ProjectUserData
                    Assert.That (state.TimerState.ProjectUsers.Values.SingleOrDefault (x => x.ProjectId == project.Id), Is.Not.Null);
                    Assert.That (dataStore.Table<ProjectUserData> ().SingleOrDefault (x => x.ProjectId == project.Id), Is.Not.Null);

                    tcs.SetResult (true);
                } catch (Exception ex) {
                    tcs.SetException (ex);
                }
            }));
            await tcs.Task;
        }


        [Test]
        public async Task TestSetClient ()
        {
            var pcolor = 5;
            var pname = "MyProject2";
            var client = new ClientData {
                Id = Guid.NewGuid (),
                Name = "MyClient"
            };
            var tcs = Util.CreateTask<bool> ();

            viewModel.SetClient (client);
            viewModel.SaveProject (pname, pcolor, new SyncTestOptions (false, (state, sent, queued) => {
                try {
                    Assert.That (state.TimerState.Projects.Values.SingleOrDefault (
                                     x => x.Name == pname && x.ClientId == client.Id), Is.Not.Null);

                    tcs.SetResult (true);
                } catch (Exception ex) {
                    tcs.SetException (ex);
                }
            }));

            await tcs.Task;
        }
    }
}
