import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeAgo',
})
export class TimeAgoPipe implements PipeTransform {
  transform(value?: string | Date | null) {
    if (!value) return '';

    const date = new Date(value);
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);

    if (Number.isNaN(seconds)) return '';
    if (seconds < 60) return 'just now';

    const intervals = [
      { label: 'year', seconds: 31536000 },
      { label: 'month', seconds: 2592000 },
      { label: 'week', seconds: 604800 },
      { label: 'day', seconds: 86400 },
      { label: 'hour', seconds: 3600 },
      { label: 'minute', seconds: 60 },
    ];

    const interval = intervals.find(x => seconds >= x.seconds);
    if (!interval) return 'just now';

    const count = Math.floor(seconds / interval.seconds);
    return `${count} ${interval.label}${count === 1 ? '' : 's'} ago`;
  }
}
