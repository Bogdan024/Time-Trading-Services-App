import { Component, ElementRef, OnDestroy, effect, input, viewChild } from '@angular/core';
import * as L from 'leaflet';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-task-location-map',
  imports: [],
  templateUrl: './task-location-map.html',
  styleUrl: './task-location-map.css',
})
export class TaskLocationMap implements OnDestroy {
  latitude = input<number | null | undefined>();
  longitude = input<number | null | undefined>();
  label = input('Task location');

  protected mapElement = viewChild<ElementRef<HTMLDivElement>>('map');
  private map?: L.Map;
  private marker?: L.Marker;

  constructor() {
    effect(() => {
      const element = this.mapElement();
      const latitude = this.latitude();
      const longitude = this.longitude();

      if (!element || latitude == null || longitude == null) return;

      queueMicrotask(() => this.renderMap(element.nativeElement, latitude, longitude));
    });
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private renderMap(element: HTMLDivElement, latitude: number, longitude: number) {
    const coordinates: L.LatLngExpression = [latitude, longitude];

    if (!this.map) {
      this.map = L.map(element, {
        attributionControl: true,
        zoomControl: true,
        scrollWheelZoom: false,
      }).setView(coordinates, 13);

      L.tileLayer(`https://maps.geoapify.com/v1/tile/osm-bright/{z}/{x}/{y}.png?apiKey=${environment.geoapifyApiKey}`, {
        maxZoom: 20,
        attribution: 'Powered by Geoapify | © OpenStreetMap contributors',
      }).addTo(this.map);
    } else {
      this.map.setView(coordinates, 13);
      this.marker?.remove();
    }

    const icon = L.divIcon({
      className: 'task-location-pin',
      html: '<span></span>',
      iconSize: [30, 30],
      iconAnchor: [15, 15],
    });

    this.marker = L.marker(coordinates, { icon }).addTo(this.map).bindPopup(this.label());
    this.map.invalidateSize();
  }
}

