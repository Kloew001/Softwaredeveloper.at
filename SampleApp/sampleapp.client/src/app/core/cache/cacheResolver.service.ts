import { Injectable } from '@angular/core';
import { map, shareReplay } from 'rxjs';

export class CacheItem<T> {
  constructor(
    public value: T,
    public expiresIn: Date,
    public valueAction: any) { }
}

export class BaseCacheResolverService<T> {
  constructor() { }

  protected cache = new Map<String, CacheItem<T>>();
  
  resolve(key, valueAction, reload: boolean | null = null, timeToLeave: number | null = null) {

    var cache$ = this.get(key);

    if (cache$ && reload == true) {
      this.cache.delete(key);
      cache$ = null;
    }

    if (!cache$) {
      cache$ = valueAction()
        .pipe(
          shareReplay(1)
        );
      this.set(key, cache$, valueAction, timeToLeave);
    }

    return cache$;
  }

  set(key, value, valueAction, timeToLeave: number | null = null) {
    if (timeToLeave) {
      const expiresIn = new Date;
      expiresIn.setSeconds(expiresIn.getSeconds() + timeToLeave);
      this.cache.set(key, new CacheItem(value, expiresIn, valueAction));
    }
    else {
      this.cache.set(key, new CacheItem(value, null, valueAction));
    }
  }

  get(key) {
    var cache$ = this.cache.get(key);

    if (!cache$)
      return null;

    const now = new Date();

    if (cache$.expiresIn && cache$.expiresIn.getTime() < now.getTime()) {
      this.cache.delete(key);
      return null;
    }

    return cache$.value;
  }

  clearAll() {
    this.cache.clear();
  }
}

@Injectable()
export class CacheResolverService extends BaseCacheResolverService<any> {
  constructor() {
    super();
  }
}
