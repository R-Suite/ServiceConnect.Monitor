define(['backbone',
    'app/views/endpoints',
    'app/views/auditMessages',
    'app/views/endpointDetails',
    'app/views/navigation',
    'app/collections/heartbeats',
    'app/models/endpoint',
    'moment'
], function(Backbone, EndpointsView, AuditsView, EndpointDetailsView, Navigation, HeartbeatCollection, EndpointModel, moment) {

    "use strict";

    var router = Backbone.Router.extend({

        routes: {
            "": "endpoints",
            "endpoints(/)": "endpoints",
            "endpoint/:name/:location": "endpointDetails",
            "audit(/)": "audits"
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

        audits: function() {
            var view = new AuditsView();
            this.renderView(view);
        },

        endpointDetails: function(name, location) {
            var that = this;
            var collection = new HeartbeatCollection();
            var model = new EndpointModel();
            var modelPromise = model.fetch({
                data: {
                    name: name,
                    location: location
                }
            });
            var collectionPromise = collection.fetch({
                data: {
                    name: name,
                    location: location,
                    from: moment.utc().subtract(1, "hours").format(),
                    to: moment.utc().format()
                }
            });
            $.when(modelPromise, collectionPromise).done(function() {
                model.set({
                    From: moment.utc().subtract(1, "hours").format(),
                    To: moment.utc().format()
                });
                var view = new EndpointDetailsView({
                    collection: collection,
                    model: model
                });
                that.renderView(view);
            });
        }
    });

    return router;
});
