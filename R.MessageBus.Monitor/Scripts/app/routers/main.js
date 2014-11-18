define([
    'backbone',
    'app/views/endpoints',
    'app/views/endpointDetails',
    'app/views/navigation',
    'app/models/endpoint'
], function(Backbone, EndpointsView, EndpointDetailsView, Navigation, EndpointModel) {

    var router = Backbone.Router.extend({

        routes: {
            "": "endpoints",
            "endpoints(/)": "endpoints",
            "endpoint/:name": "endpointDetails"
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

        endpointDetails: function(name) {
            var that = this;
            var model = new EndpointModel();
            model.set("id", name);
            model.fetch({
                success: function() {
                    var view = new EndpointDetailsView({
                        model: model
                    });
                    that.renderView(view);
                }
            });
        }
    });

    return router;
});
