import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { PaginatedResult } from '../../types/pagination';
import { CreateTimeTask, TaskParams, TimeTask, TimeTransaction, UpdateTimeTask } from '../../types/task';

@Injectable({
  providedIn: 'root',
})
export class TaskService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getTasks(taskParams = new TaskParams()) {
    let params = new HttpParams()
      .set('pageNumber', taskParams.pageNumber)
      .set('pageSize', taskParams.pageSize)
      .set('orderBy', taskParams.orderBy);

    if (taskParams.serviceCategoryId) params = params.set('serviceCategoryId', taskParams.serviceCategoryId);
    if (taskParams.locationMode) params = params.set('locationMode', taskParams.locationMode);
    if (taskParams.city) params = params.set('city', taskParams.city);
    if (taskParams.countryCode) params = params.set('countryCode', taskParams.countryCode);
    if (taskParams.minCredits) params = params.set('minCredits', taskParams.minCredits);
    if (taskParams.maxCredits) params = params.set('maxCredits', taskParams.maxCredits);
    if (taskParams.dueSoon) params = params.set('dueSoon', taskParams.dueSoon);
    if (taskParams.minPosterRating) params = params.set('minPosterRating', taskParams.minPosterRating);

    return this.http.get<PaginatedResult<TimeTask>>(this.baseUrl + 'tasks', { params });
  }

  getTask(id: number) {
    return this.http.get<TimeTask>(this.baseUrl + 'tasks/' + id);
  }

  getMyTasks() {
    return this.http.get<TimeTask[]>(this.baseUrl + 'tasks/mine');
  }

  getAcceptedTasks() {
    return this.http.get<TimeTask[]>(this.baseUrl + 'tasks/accepted');
  }

  getTransactions() {
    return this.http.get<TimeTransaction[]>(this.baseUrl + 'tasks/transactions');
  }

  createTask(task: CreateTimeTask) {
    return this.http.post<TimeTask>(this.baseUrl + 'tasks', task);
  }

  updateTask(id: number, task: UpdateTimeTask) {
    return this.http.put<void>(this.baseUrl + 'tasks/' + id, task);
  }

  acceptTask(id: number) {
    return this.http.patch<void>(this.baseUrl + 'tasks/' + id + '/accept', {});
  }

  completeTask(id: number) {
    return this.http.patch<TimeTransaction>(this.baseUrl + 'tasks/' + id + '/complete', {});
  }

  cancelTask(id: number) {
    return this.http.patch<void>(this.baseUrl + 'tasks/' + id + '/cancel', {});
  }
}

