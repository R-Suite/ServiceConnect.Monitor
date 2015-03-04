define(['backbone'], function(Backbone) {

    "use strict";

    var model = Backbone.Model.extend({
        idAttribute: 'Id',
        urlRoot: "error",

        retry: function(options) {
            $.ajax({
                type: "POST",
                url: "errors/retry",
                success: options.success,
                error: options.error,
                data: JSON.stringify(new Backbone.Collection(this.attributes).toJSON()),
                traditional: true,
                dataType: 'json',
                contentType: 'application/json'
            });
        }
    });

    return model;
});
