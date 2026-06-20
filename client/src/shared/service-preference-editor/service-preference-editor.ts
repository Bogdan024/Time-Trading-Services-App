import { Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MemberServiceCategoryEdit, ServiceCategory, ServicePreferenceItem } from '../../types/member';

@Component({
  selector: 'app-service-preference-editor',
  imports: [ReactiveFormsModule],
  templateUrl: './service-preference-editor.html',
  styleUrl: './service-preference-editor.css',
})
export class ServicePreferenceEditor {
  private fb = inject(FormBuilder);

  title = input('');
  description = input('');
  emptyText = input('No categories selected yet.');
  addButtonText = input('Add');
  canEdit = input(true);
  layout = input<'compact' | 'wide'>('wide');
  categories = input<ServiceCategory[]>([]);
  items = input<ServicePreferenceItem[]>([]);
  addItem = output<MemberServiceCategoryEdit>();
  removeItem = output<ServicePreferenceItem>();

  protected form = this.fb.group({
    serviceCategoryId: [0],
    note: ['', Validators.maxLength(250)],
  });

  protected availableCategories() {
    const selectedCategoryIds = new Set(this.items().map((item) => item.serviceCategoryId));

    return this.categories().filter((category) => !selectedCategoryIds.has(category.id));
  }

  protected addPreference() {
    const serviceCategoryId = Number(this.form.value.serviceCategoryId);

    if (!serviceCategoryId) return;

    this.addItem.emit({
      serviceCategoryId,
      note: this.form.value.note?.trim() || undefined,
    });
    this.form.reset({ serviceCategoryId: 0, note: '' });
  }

  protected categoryName(item: ServicePreferenceItem) {
    return item.serviceCategory?.name ?? this.categories().find((category) => category.id === item.serviceCategoryId)?.name ?? 'Category';
  }

}


