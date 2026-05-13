import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { ServiceCategory } from '../../types/member';

@Injectable({
  providedIn: 'root',
})
export class ServiceCategoryService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getServiceCategories() {
    return this.http.get<ServiceCategory[]>(this.baseUrl + 'servicecategories');
  }
}
