FROM microsoft/dotnet:2.1-sdk AS build-env
ARG EXE_DIR=.
WORKDIR /app
COPY $EXE_DIR/ ./
RUN useradd -ms /bin/bash moduleuser
USER moduleuser
ENV VRTU_CONFIG_SAS_URI https://virtualrtu.blob.core.windows.net/config/vconfig.json?sp=r&st=2018-08-03T15:15:54Z&se=2020-01-02T00:15:54Z&spr=https&sv=2017-11-09&sig=zMWtGHSYTWIyNqsPfeJ3tsSKN9IOnUI5O9j4jJxKN9g%3D&sr=b
ENV RTU_MAP_SAS_URI https://virtualrtu.blob.core.windows.net/map/rtumap.json?st=2018-08-18T16%3A45%3A59Z&se=2018-08-19T16%3A45%3A59Z&sp=r&sv=2018-03-28&sr=b&sig=m%2BA30aXHehJIgwSKhin9AgiXj4tyQx0XgKPMoRQLx28%3D
CMD ["dotnet", "SkunkLab.VirtualRtu.Listener.dll"]
