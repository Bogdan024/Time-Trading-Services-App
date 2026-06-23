import { Component } from '@angular/core';
import { UserManagement } from './user-management/user-management';

@Component({
  selector: 'app-admin',
  imports: [UserManagement],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export class Admin {}
