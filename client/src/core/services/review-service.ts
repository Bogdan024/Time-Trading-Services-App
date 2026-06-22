import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { CreateReview, MemberReview, ReviewSummary } from '../../types/review';

@Injectable({
  providedIn: 'root',
})
export class ReviewService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  createReview(taskId: number, review: CreateReview) {
    return this.http.post<MemberReview>(this.baseUrl + 'reviews/tasks/' + taskId, review);
  }

  getMemberReviews(memberId: string) {
    return this.http.get<MemberReview[]>(this.baseUrl + 'reviews/members/' + memberId);
  }

  getMemberReviewSummary(memberId: string) {
    return this.http.get<ReviewSummary>(this.baseUrl + 'reviews/members/' + memberId + '/summary');
  }

  getReviewedTaskIds() {
    return this.http.get<number[]>(this.baseUrl + 'reviews/mine/task-ids');
  }
}
