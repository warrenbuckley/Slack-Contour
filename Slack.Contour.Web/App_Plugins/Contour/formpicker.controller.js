angular.module("umbraco").controller("Contour.FormPickerController",
        function ($scope, $http) {
            

            $scope.loading = true;
            $http({ method: 'GET', url: 'Contour/FormsApi/GetAll' })
                .success(function (data) {
                    $scope.forms  = data;
                    $scope.loading = false;
                })
                .error(function () {
                    $scope.error = "An Error has occured while loading!";
                    $scope.loading = false;
                });
        });