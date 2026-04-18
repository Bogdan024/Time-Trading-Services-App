import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private http = inject(HttpClient);
  protected readonly title = 'Time Trading App';
  protected readonly members = signal<any[]>([]);

  async ngOnInit() {
    this.members.set(await this.getMembers());
  }

  private async getMembers() {
    return firstValueFrom(
      this.http.get<any[]>('https://localhost:5001/api/members')
    );
  }
}
