define(['backbone', "jquery", "app/models/error", 'backbone-pageable'], function(Backbone, $, ErrorModel) {

    "use strict";

    var collection = Backbone.PageableCollection.extend({
        url: "errors",
        mode: "client",
        model: ErrorModel,

        retry: function(options) {
            $.ajax({
                type: "POST",
                url: this.url + "/retry",
                success: options.success,
                error: options.error,
                data: JSON.stringify(this.fullCollection.toJSON()),
                traditional: true,
                dataType: 'json',
                contentType: 'application/json'
            });
        }
    });

    return collection;
});
