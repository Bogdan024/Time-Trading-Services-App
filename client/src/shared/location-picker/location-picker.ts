import { HttpClient, HttpParams } from '@angular/common/http';
import { Component, inject, input, output, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { catchError, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';
import { environment } from '../../environments/environment';
import { TaskLocation } from '../../types/task';

type GeoapifyFeature = {
  properties: {
    formatted?: string;
    city?: string;
    town?: string;
    village?: string;
    municipality?: string;
    county?: string;
    state?: string;
    country_code?: string;
    place_id?: string;
    lat?: number;
    lon?: number;
  };
  geometry?: {
    coordinates?: [number, number];
  };
};

type GeoapifyAutocompleteResponse = {
  features?: GeoapifyFeature[];
};

@Component({
  selector: 'app-location-picker',
  imports: [ReactiveFormsModule],
  templateUrl: './location-picker.html',
  styleUrl: './location-picker.css',
})
export class LocationPicker {
  private http = inject(HttpClient);

  label = input('Task location');
  helperText = input('Choose a suggestion so city, country, and map coordinates are saved correctly.');
  placeholder = input('Search a city, address, or neighbourhood');
  selectedLabel = input('Selected location');
  locationSelected = output<TaskLocation>();
  locationCleared = output<void>();
  protected searchControl = new FormControl('', { nonNullable: true });
  protected suggestions = signal<TaskLocation[]>([]);
  protected selectedLocation = signal<TaskLocation | null>(null);
  protected loading = signal(false);
  protected error = signal('');

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(350),
        map((value) => value.trim()),
        distinctUntilChanged(),
        switchMap((value) => this.searchLocations(value)),
      )
      .subscribe((locations) => {
        this.suggestions.set(locations);
        this.loading.set(false);
      });
  }

  protected chooseLocation(location: TaskLocation) {
    this.selectedLocation.set(location);
    this.suggestions.set([]);
    this.error.set('');
    this.searchControl.setValue(location.formattedAddress, { emitEvent: false });
    this.locationSelected.emit(location);
  }

  protected clearLocation() {
    this.selectedLocation.set(null);
    this.searchControl.setValue('', { emitEvent: false });
    this.suggestions.set([]);
    this.error.set('');
    this.locationCleared.emit();
  }

  private searchLocations(query: string) {
    this.error.set('');

    if (this.selectedLocation()) {
      this.locationCleared.emit();
    }

    this.selectedLocation.set(null);

    if (query.length < 3) {
      this.suggestions.set([]);
      return of([]);
    }

    this.loading.set(true);

    const params = new HttpParams()
      .set('text', query)
      .set('format', 'geojson')
      .set('limit', 6)
      .set('apiKey', environment.geoapifyApiKey);

    return this.http.get<GeoapifyAutocompleteResponse>('https://api.geoapify.com/v1/geocode/autocomplete', { params }).pipe(
      map((response) => (response.features ?? []).map((feature) => this.toTaskLocation(feature)).filter((location): location is TaskLocation => !!location)),
      catchError(() => {
        this.error.set('Could not load location suggestions. Please try again.');
        return of([]);
      }),
    );
  }

  private toTaskLocation(feature: GeoapifyFeature): TaskLocation | null {
    const properties = feature.properties;
    const longitude = properties.lon ?? feature.geometry?.coordinates?.[0];
    const latitude = properties.lat ?? feature.geometry?.coordinates?.[1];
    const countryCode = properties.country_code?.toUpperCase();
    const city = properties.city ?? properties.town ?? properties.village ?? properties.municipality ?? properties.county ?? properties.state;
    const formattedAddress = properties.formatted;

    if (!city || !countryCode || !formattedAddress || latitude === undefined || longitude === undefined) {
      return null;
    }

    return {
      city,
      countryCode,
      formattedAddress,
      latitude,
      longitude,
      placeId: properties.place_id,
    };
  }
}
