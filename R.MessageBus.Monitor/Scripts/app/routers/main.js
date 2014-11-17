define([
    'backbone',
    'app/views/endpoints',
    'app/views/navigation'
], function(Backbone, EndpointsView, Navigation) {

    var router = Backbone.Router.extend({

        routes: {
            "": "endpoints",
            "dashboard(/)": "endpoints"
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
        }
    });

    return router;
});
