import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { TaskList } from '../features/tasks/task-list/task-list';
import { TaskDetail } from '../features/tasks/task-detail/task-detail';
import { MyTasks } from '../features/tasks/my-tasks/my-tasks';
import { GroupList } from '../features/groups/group-list/group-list';
import { GroupDetail } from '../features/groups/group-detail/group-detail';
import { Messages } from '../features/messages/messages';
import { authGuard } from '../core/guards/auth-guard';
import { TestErrors } from '../features/test-errors/test-errors';
import { NotFound } from '../shared/errors/not-found/not-found';
import { ServerError } from '../shared/errors/server-error/server-error';

export const routes: Routes = [
  { path: '', component: Home },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [authGuard],
    children: [
      { path: 'tasks', component: TaskList },
      { path: 'tasks/:id', component: TaskDetail },
      { path: 'my-tasks', component: MyTasks },
      { path: 'groups', component: GroupList },
      { path: 'groups/:id', component: GroupDetail },
      { path: 'messages', component: Messages },
    ],
  },
  { path: 'errors', component: TestErrors },
  { path: 'not-found', component: NotFound },
  { path: 'server-error', component: ServerError },
  { path: '**', component: NotFound },
];
