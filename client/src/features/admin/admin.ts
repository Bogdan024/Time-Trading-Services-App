import { Component } from '@angular/core';
import { UserManagement } from './user-management/user-management';
import { ModerationQueue } from './moderation-queue/moderation-queue';

@Component({
  selector: 'app-admin',
  imports: [UserManagement, ModerationQueue],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export class Admin {}
