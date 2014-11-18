define(["backbone", 'app/views/endpoints', "sinon", 'app/views/services', 'app/views/endpointGraph'], function (Backbone, EndpointsView, sinon, ServicesView, EndpointGraphView) {

    describe("Endpoints View", function () {

        var view, server;

        describe("Render", function() {

            beforeEach(function () {
                view = new EndpointsView();
                spyOn($.prototype, "html");
                spyOn(ServicesView.prototype, "render");
                spyOn(EndpointGraphView.prototype, "render");
                server = sinon.fakeServer.create();
                
                server.respondWith("GET", "serviceMessages",
                    [
                        200,
                        { "Content-Type": "application/json" },
                        '{"id":321,"Name":"TestServiceMessage"}'
                    ]
                );

                server.respondWith("GET", "endpoints",
                    [
                        200,
                        { "Content-Type": "application/json" },
                        '{"id":123,"Name":"TestService"}'
                    ]
                );
            });

            it("should add html template to page", function () {
                view.render();
                expect($.prototype.html).toHaveBeenCalled();
            });

            it("should get a list of endpoints", function () {
                view.render();
                server.respond();
                expect(view.endpointCollection).not.toBeNull();
                expect(view.endpointCollection.length).toEqual(1);
                expect(view.endpointCollection.first().get("Name")).toEqual("TestService");
                expect(view.endpointCollection.first().get("id")).toEqual(123);
            });

            it("should get a list of service messages", function () {
                view.render();
                server.respond();
                expect(view.serviceMessagesCollection).not.toBeNull();
                expect(view.serviceMessagesCollection.length).toEqual(1);
                expect(view.serviceMessagesCollection.first().get("Name")).toEqual("TestServiceMessage");
                expect(view.serviceMessagesCollection.first().get("id")).toEqual(321);
            });

            it("should render the service table view", function() {
                view.render();
                server.respond();
                expect(ServicesView.prototype.render).toHaveBeenCalled();
                for (var i = 0; i < view.activeViews.length; i++) {
                    if (view.activeViews[i] instanceof ServicesView) {
                        expect(view.activeViews[i].collection.first().get("Name")).toEqual("TestService");
                        expect($.prototype.html).toHaveBeenCalledWith(view.activeViews[i].$el);
                    }
                }
            });

            it("should render the endpoint graph view", function() {
                view.render();
                server.respond();
                expect(EndpointGraphView.prototype.render).toHaveBeenCalled();
                for (var i = 0; i < view.activeViews.length; i++) {
                    if (view.activeViews[i] instanceof EndpointGraphView) {
                        expect(view.activeViews[i].serviceMessagesCollection.first().get("Name")).toEqual("TestServiceMessage");
                        expect(view.activeViews[i].endpointCollection.first().get("Name")).toEqual("TestService");
                        expect($.prototype.html).toHaveBeenCalledWith(view.activeViews[i].$el);
                    }
                }
            });
        });
            
    });
});