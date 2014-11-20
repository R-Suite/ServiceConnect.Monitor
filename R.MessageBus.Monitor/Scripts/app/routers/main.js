define(['backbone',
    'app/views/endpoints',
    'app/views/endpointDetails',
    'app/views/navigation',
    'app/collections/heartbeats',
    'moment'
], function(Backbone, EndpointsView, EndpointDetailsView, Navigation, HeartbeatCollection, moment) {

    "use strict";

    var router = Backbone.Router.extend({

        routes: {
            "": "endpoints",
            "endpoints(/)": "endpoints",
            "endpoint/:name/:location": "endpointDetails"
        },

        before: function(route) {
            if (!this.navigation) {
                this.navigation = new Navigation();
                this.navigation.render();
            }
            if (route === '') {
                route = 'endpoints';
            }
            this.navigation.setActive(route);

            this.closeViews();
        },

        endpoints: function() {
            var view = new EndpointsView();
            this.renderView(view);
        },

        endpointDetails: function(name, location) {
            var that = this;
            var collection = new HeartbeatCollection();
            var model = new Backbone.Model({
                Name: name,
                Location: location,
                From: moment.utc().subtract(1, "hours").format(),
                To: moment.utc().format()
            });
            collection.fetch({
                data: model.attributes,
                success: function() {
                    var view = new EndpointDetailsView({
                        collection: collection,
                        model: model
                    });
                    that.renderView(view);
                }
            });
        }
    });

    return router;
});
