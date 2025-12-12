$version = minver -i -t v -v w
docker tag benchmarkdotnet-docfx:$version jcr.codebelt.net/geekle/benchmarkdotnet-docfx:$version
docker push jcr.codebelt.net/geekle/benchmarkdotnet-docfx:$version
