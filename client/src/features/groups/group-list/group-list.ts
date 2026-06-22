import { Component, signal } from '@angular/core';
import { ToastService } from '../../../core/services/toast-service';
import { inject } from '@angular/core';

type CommunityGroup = {
  id: string;
  name: string;
  members: number;
  description: string;
  color: string;
  joined: boolean;
};

@Component({
  selector: 'app-group-list',
  imports: [],
  templateUrl: './group-list.html',
  styleUrl: './group-list.css',
})
export class GroupList {
  private toast = inject(ToastService);

  protected groups = signal<CommunityGroup[]>([
    { id: 'g1', name: 'Timișoara Tool Library', members: 124, color: '#b5703f', joined: true, description: 'Share drills, ladders and garden tools instead of everyone buying their own.' },
    { id: 'g2', name: 'Weekend Gardeners', members: 58, color: '#6f8a4f', joined: false, description: 'Trade plant care, seeds and a few hours of weeding around the city.' },
    { id: 'g3', name: 'Language Exchange TM', members: 92, color: '#4f7a8a', joined: true, description: 'Swap conversation practice across languages, an hour for an hour.' },
    { id: 'g4', name: 'Senior Tech Buddies', members: 41, color: '#8a5a7a', joined: false, description: 'Patient, friendly tech help for older neighbours nearby.' },
    { id: 'g5', name: 'Neighborhood Cooks', members: 73, color: '#b5654a', joined: false, description: 'Cook together, share meals and bank a few credits in the process.' },
    { id: 'g6', name: 'Bike Fixers Co-op', members: 36, color: '#7a6a9a', joined: false, description: 'Roadside fixes, flat tyres and basic maintenance on weekends.' },
  ]);

  protected initial(name: string) {
    return name.trim()[0]?.toUpperCase() ?? '?';
  }

  protected toggleJoin(group: CommunityGroup) {
    this.groups.update(groups =>
      groups.map(g =>
        g.id === group.id
          ? { ...g, joined: !g.joined, members: g.joined ? g.members - 1 : g.members + 1 }
          : g,
      ),
    );
    const updated = this.groups().find(g => g.id === group.id);
    this.toast.success(updated?.joined ? `Joined ${group.name}` : `Left ${group.name}`);
  }
}
