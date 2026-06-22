import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CreateReview } from '../../types/review';

@Component({
  selector: 'app-review-form',
  imports: [FormsModule],
  templateUrl: './review-form.html',
  styleUrl: './review-form.css',
})
export class ReviewForm {
  taskTitle = input.required<string>();
  targetName = input.required<string>();
  loading = input(false);
  submitReview = output<CreateReview>();
  cancelReview = output<void>();

  protected rating = 5;
  protected hoverRating = 0;
  protected comment = '';
  protected readonly stars = [1, 2, 3, 4, 5];

  protected displayRating() {
    return this.hoverRating || this.rating;
  }

  protected selectRating(rating: number) {
    if (this.loading()) return;

    this.rating = rating;
  }

  protected previewRating(rating: number) {
    if (this.loading()) return;

    this.hoverRating = rating;
  }

  protected clearPreview() {
    this.hoverRating = 0;
  }

  protected ratingLabel() {
    return {
      1: 'Needs work',
      2: 'Could be better',
      3: 'Good exchange',
      4: 'Great exchange',
      5: 'Excellent exchange',
    }[this.displayRating()] ?? 'Select a rating';
  }

  protected submit() {
    this.submitReview.emit({
      rating: this.rating,
      comment: this.comment.trim() || undefined,
    });
  }
}
