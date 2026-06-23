import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { TaskList } from '../features/tasks/task-list/task-list';
import { TaskDetail } from '../features/tasks/task-detail/task-detail';
import { TaskCreate } from '../features/tasks/task-create/task-create';
import { MyTasks } from '../features/tasks/my-tasks/my-tasks';
import { PostedTasks } from '../features/tasks/posted-tasks/posted-tasks';
import { AcceptedTasks } from '../features/tasks/accepted-tasks/accepted-tasks';
import { TaskHistory } from '../features/tasks/task-history/task-history';
import { GroupList } from '../features/groups/group-list/group-list';
import { GroupDetail } from '../features/groups/group-detail/group-detail';
import { Messages } from '../features/messages/messages';
import { MemberList } from '../features/members/member-list/member-list';
import { MemberDetail } from '../features/members/member-detail/member-detail';
import { memberResolver } from '../features/members/member-resolver';
import { authGuard } from '../core/guards/auth-guard';
import { adminGuard } from '../core/guards/admin-guard';
import { preventUnsavedChangesGuard } from '../core/guards/prevent-unsaved-changes-guard';
import { TestErrors } from '../features/test-errors/test-errors';
import { NotFound } from '../shared/errors/not-found/not-found';
import { ServerError } from '../shared/errors/server-error/server-error';
import { Admin } from '../features/admin/admin';

export const routes: Routes = [
  { path: '', component: Home },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [authGuard],
    children: [
      { path: 'tasks', component: TaskList },
      { path: 'tasks/create', component: TaskCreate },
      { path: 'tasks/:id', component: TaskDetail },
      {
        path: 'my-tasks',
        component: MyTasks,
        children: [
          { path: 'posted', component: PostedTasks },
          { path: 'accepted', component: AcceptedTasks },
          { path: 'history', component: TaskHistory },
          { path: '', redirectTo: 'posted', pathMatch: 'full' },
        ],
      },
      { path: 'members', component: MemberList },
      {
        path: 'members/:id',
        component: MemberDetail,
        resolve: { member: memberResolver },
        canDeactivate: [preventUnsavedChangesGuard],
      },
      { path: 'groups', component: GroupList },
      { path: 'groups/:id', component: GroupDetail },
      { path: 'messages', component: Messages },
      { path: 'admin', component: Admin, canActivate: [adminGuard] },
    ],
  },
  { path: 'errors', component: TestErrors },
  { path: 'not-found', component: NotFound },
  { path: 'server-error', component: ServerError },
  { path: '**', component: NotFound },
];



