\self.addEventListener('install', function (event) {
    console.log('[Service Worker] Installed');
    event.waitUntil(
        caches.open('pakprops-cache').then(function (cache) {
            return cache.addAll([
                '/',
                '/css/site.css',
                '/js/site.js',
                '/images/icon-192.png'
            ]);
        })
    );
});

self.addEventListener('fetch', function (event) {
    event.respondWith(
        caches.match(event.request).then(function (response) {
            return response || fetch(event.request);
        })
    );
});
