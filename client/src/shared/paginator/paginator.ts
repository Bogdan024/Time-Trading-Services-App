import { Component, computed, input, model, output } from '@angular/core';

@Component({
  selector: 'app-paginator',
  imports: [],
  templateUrl: './paginator.html',
  styleUrl: './paginator.css',
})
export class Paginator {
  pageNumber = model(1);
  pageSize = model(9);
  totalCount = input(0);
  totalPages = input(0);
  pageSizeOptions = input([6, 9, 12, 24]);
  pageChange = output<{ pageNumber: number; pageSize: number }>();

  protected lastItemIndex = computed(() => Math.min(this.pageNumber() * this.pageSize(), this.totalCount()));

  protected onPageChange(pageNumber?: number, pageSize?: EventTarget | null) {
    if (pageNumber) this.pageNumber.set(pageNumber);

    if (pageSize) {
      this.pageSize.set(Number((pageSize as HTMLSelectElement).value));
      this.pageNumber.set(1);
    }

    this.pageChange.emit({
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
    });
  }
}
