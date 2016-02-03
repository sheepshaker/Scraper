(function() {
    'use strict';
    angular.module('app').controller('MainCtrl', main);
    

    function main() {
	var vm = this;
	vm.food = 'pizza';
    }
})();