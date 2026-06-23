import { Component } from '@angular/core';
import { HasRole } from '../../shared/directives/has-role';
import { UserManagement } from './user-management/user-management';
import { ModerationQueue } from './moderation-queue/moderation-queue';

@Component({
  selector: 'app-admin',
  imports: [UserManagement, ModerationQueue, HasRole],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export class Admin {}

