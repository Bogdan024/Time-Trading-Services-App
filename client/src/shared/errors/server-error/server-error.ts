import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiError } from '../../../types/error';

@Component({
  selector: 'app-server-error',
  imports: [],
  templateUrl: './server-error.html',
  styleUrl: './server-error.css',
})
export class ServerError {
  private router = inject(Router);
  protected showDetails = false;
  protected error: ApiError = {
    statusCode: 500,
    message: 'Server error',
    details: 'No error details were provided.',
  };

  constructor() {
    const navigation = this.router.getCurrentNavigation();
    this.error = navigation?.extras?.state?.['error'] ?? this.error;
  }

  detailsToggle() {
    this.showDetails = !this.showDetails;
  }
}
