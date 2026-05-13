import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { CreateTimeTask, TimeTask, TimeTransaction, UpdateTimeTask } from '../../types/task';

@Injectable({
  providedIn: 'root',
})
export class TaskService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getTasks() {
    return this.http.get<TimeTask[]>(this.baseUrl + 'tasks');
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
