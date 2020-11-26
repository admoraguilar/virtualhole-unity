EditorStatsReporting
====================

June 2020: This is a fork of the MIT-licensed "google-measurement-protocol-dotnet".

I have moved it to its own namespace so that it will not collide with your code if you're using the GMP in your game (e.g. if you use GMP to do game-design analytics - unlike Unity Analytics, its free!).

Note that this version is customised to remove some of the features - we are NOT using Google-tracking, we're only using it for reporting anonymous usage stats - so I deleted some pieces we don't want.

## Upstream version (pre fork)

```
Install-Package GoogleMeasurementProtocol

```

License
----

MIT

References
----

[Measurement Protocol Developer Guide]:https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
[Measurement Protocol Parameter Reference]:https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
