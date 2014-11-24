define(['backbone'], function(Backbone) {

    "use strict";

    var model = Backbone.Model.extend({
        idAttribute: 'Id',
        url: "service"
    });

    return model;
});
