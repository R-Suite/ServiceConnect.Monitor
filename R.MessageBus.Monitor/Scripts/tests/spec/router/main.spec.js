define(["backbone",
        "sinon",
        'app/routers/main',
        'app/views/endpoints',
        'app/views/navigation',
        'app/views/endpointDetails',
        'app/models/endpoint'],
    function (Backbone,
        sinon,
        MainRouter,
        EndpointsView,
        Navigation,
        EndpointDetails,
        EndpointModel) {

    describe("Main Router", function () {

        var router;

        describe("Routes", function() {

            beforeEach(function () {
                spyOn(MainRouter.prototype, 'endpoints');
                spyOn(MainRouter.prototype, 'endpointDetails');
                spyOn(MainRouter.prototype.__proto__, 'closeViews');
                spyOn(Navigation.prototype, 'render');
                spyOn(Navigation.prototype, 'setActive');
                router = new MainRouter();
                Backbone.history.start();
            });

            it("empty route routes to endpoints", function () {
                Backbone.history.navigate('', { trigger: true });
                expect(MainRouter.prototype.endpoints).toHaveBeenCalled();
            });

            it("endpoints route routes to endpoints", function () {
                Backbone.history.navigate('endpoints', { trigger: true });
                expect(MainRouter.prototype.endpoints).toHaveBeenCalled();
            });
            
            it("endpoint/:name route routes to endpointDetails", function () {
                Backbone.history.navigate('endpoint/test', { trigger: true });
                expect(MainRouter.prototype.endpointDetails).toHaveBeenCalled();
            });

            it("closeViews method called when routing", function () {
                Backbone.history.navigate('', { trigger: true });
                expect(MainRouter.prototype.__proto__.closeViews).toHaveBeenCalled();
            });

            it("should render navigation menu when routing", function() {
                Backbone.history.navigate('', { trigger: true });
                expect(Navigation.prototype.render).toHaveBeenCalled();
            });

            it("should not render the navigation menu when routing if the navgiation view already exists", function () {
                Backbone.history.stop();
                Navigation.prototype.render.reset();
                router.navigation = new Navigation();
                Backbone.history.start();
                Backbone.history.navigate('endpoints', { trigger: true });
                expect(Navigation.prototype.render).not.toHaveBeenCalled();
            });

            it("should pass 'endpoints' to navigation when routing to empty route", function () {
                Backbone.history.navigate('', { trigger: true });
                expect(Navigation.prototype.setActive).toHaveBeenCalledWith('endpoints');
            });

            it("should pass 'endpoints' to navigation when routing to endpoints route", function() {
                Backbone.history.navigate('endpoints', { trigger: true });
                expect(Navigation.prototype.setActive).toHaveBeenCalledWith('endpoints');
            });

            afterEach(function () {
                Backbone.history.navigate('', { trigger: false });
                Backbone.history.stop();
            });
        });

        describe("Endpoint Route", function() {

            beforeEach(function () {
                spyOn(EndpointsView.prototype, 'render');
                router = new MainRouter();
                Backbone.history.start();
            });

            it("dashbooard route should render endpoint view", function () {
                Backbone.history.navigate('', { trigger: true });
                expect(EndpointsView.prototype.render).toHaveBeenCalled();
            });

            it("empty route should render endpoint view", function () {
                Backbone.history.navigate('', { trigger: true });
                expect(EndpointsView.prototype.render).toHaveBeenCalled();
            });

            afterEach(function () {
                Backbone.history.navigate('', { trigger: false });
                Backbone.history.stop();
            });
        });
        
        describe("Endpoint/:name Route", function () {

            var spy, model, server;

            beforeEach(function () {
                spyOn(EndpointDetails.prototype, 'render');
                server = sinon.fakeServer.create();

                server.respondWith("GET", "endpoint/test",
                    [
                        200,
                        { "Content-Type": "application/json" },
                        '{"id":"test", "Name":"test"}'
                    ]
                );
                
                spy = sinon.stub(EndpointDetails.prototype, "initialize", function (a) {
                    model = a.model;
                    return sinon.stub();
                });

                router = new MainRouter();
                Backbone.history.start();
            });

            it("endpoint details route should render endpoint view", function () {
                Backbone.history.navigate('endpoint/test', { trigger: true });
                server.respond();
                expect(EndpointDetails.prototype.render).toHaveBeenCalled();
            });
            
            it("endpoint details should pass model to view", function () {
                Backbone.history.navigate('endpoint/test', { trigger: true });
                server.respond();
                expect(EndpointDetails.prototype.initialize.called).toBeTruthy();
                expect(model.get("id")).toEqual("test");
                expect(model.get("Name")).toEqual("test");
            });

            afterEach(function () {
                Backbone.history.navigate('', { trigger: false });
                Backbone.history.stop();
                EndpointDetails.prototype.initialize.restore();
            });
        });
            
    });
});