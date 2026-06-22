import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ServiceCategoryService } from '../../../core/services/service-category-service';
import { TaskService } from '../../../core/services/task-service';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { Paginator } from '../../../shared/paginator/paginator';
import { PaginatedResult } from '../../../types/pagination';
import { ServiceCategory } from '../../../types/member';
import { TaskParams, TimeTask } from '../../../types/task';
import { TaskFilters } from '../task-filters/task-filters';

@Component({
  selector: 'app-task-list',
  imports: [DatePipe, RouterLink, TaskFilters, Paginator, TimeAgoPipe],
  templateUrl: './task-list.html',
  styleUrl: './task-list.css',
})
export class TaskList implements OnInit {
  private taskService = inject(TaskService);
  private serviceCategoryService = inject(ServiceCategoryService);
  private readonly filterStorageKey = 'taskFilters';

  protected tasks = signal<PaginatedResult<TimeTask> | null>(null);
  protected serviceCategories = signal<ServiceCategory[]>([]);
  protected taskParams = this.getStoredTaskParams();

  ngOnInit() {
    this.loadServiceCategories();
    this.loadTasks();
  }

  protected onFilterChange(taskParams: TaskParams) {
    this.taskParams = Object.assign(new TaskParams(), taskParams, {
      pageNumber: 1,
      pageSize: this.taskParams.pageSize,
    });

    this.loadTasks();
  }

  protected onPageChange(event: { pageNumber: number; pageSize: number }) {
    this.taskParams.pageNumber = event.pageNumber;
    this.taskParams.pageSize = event.pageSize;
    this.loadTasks();
  }

  protected resetFilters() {
    const pageSize = this.taskParams.pageSize;
    this.taskParams = Object.assign(new TaskParams(), { pageSize });
    localStorage.removeItem(this.filterStorageKey);
    this.loadTasks();
  }

  protected locationModeName(mode: number) {
    return {
      1: 'In person',
      2: 'Remote',
      3: 'Either',
    }[mode] ?? 'Flexible';
  }

  private readonly categoryColors: Record<string, string> = {
    moving: '#b5703f',
    repair: '#8a6d4b',
    tutoring: '#5f7355',
    gardening: '#6f8a4f',
    tech: '#4f7a8a',
    cooking: '#b5654a',
    pets: '#8a5a7a',
    errands: '#7a6a9a',
  };

  protected categoryColor(key?: string) {
    return (key && this.categoryColors[key]) || '#8a8178';
  }

  protected initial(name?: string) {
    return (name?.trim()?.[0] ?? '?').toUpperCase();
  }

  protected creditLabel(hours: number) {
    return hours === 1 ? '1 credit' : `${hours} credits`;
  }

  private loadTasks() {
    this.taskService.getTasks(this.taskParams).subscribe({
      next: response => {
        this.tasks.set(response);
        localStorage.setItem(this.filterStorageKey, JSON.stringify(this.taskParams));
      },
    });
  }

  private loadServiceCategories() {
    this.serviceCategoryService.getServiceCategories().subscribe({
      next: categories => this.serviceCategories.set(categories),
    });
  }

  private getStoredTaskParams() {
    const storedParams = localStorage.getItem(this.filterStorageKey);

    if (!storedParams) return new TaskParams();

    try {
      return Object.assign(new TaskParams(), JSON.parse(storedParams));
    } catch {
      localStorage.removeItem(this.filterStorageKey);
      return new TaskParams();
    }
  }
}
