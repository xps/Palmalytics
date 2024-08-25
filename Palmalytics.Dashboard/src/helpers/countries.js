// Returns the full country name from the 2-letter ISO code
export function getCountryName(code) {
    const regionNames = new Intl.DisplayNames(['en'], { type: 'region' });
    return regionNames.of(code); 
}

// Returns the country flag emoji (unicode) from the 2-letter ISO code
export function getCountryFlag(code) {
  const codePoints = code.toUpperCase().split('').map(
      letter => letter.codePointAt() + 127397
  );
  return String.fromCodePoint(...codePoints);
}