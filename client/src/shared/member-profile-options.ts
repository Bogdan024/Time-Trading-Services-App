export const availabilityDays = [
  { value: 0, label: 'Sunday' },
  { value: 1, label: 'Monday' },
  { value: 2, label: 'Tuesday' },
  { value: 3, label: 'Wednesday' },
  { value: 4, label: 'Thursday' },
  { value: 5, label: 'Friday' },
  { value: 6, label: 'Saturday' },
];

export const startHours = Array.from({ length: 24 }, (_, hour) => hour);
export const endHours = Array.from({ length: 24 }, (_, index) => index + 1);

export const availabilityModes = [
  { value: 1, label: 'In person' },
  { value: 2, label: 'Remote' },
  { value: 3, label: 'Either' },
];

export function dayName(dayOfWeek: number) {
  return availabilityDays.find((day) => day.value === dayOfWeek)?.label ?? 'Flexible';
}

export function modeName(mode: number) {
  return availabilityModes.find((availabilityMode) => availabilityMode.value === mode)?.label ?? 'Flexible';
}
