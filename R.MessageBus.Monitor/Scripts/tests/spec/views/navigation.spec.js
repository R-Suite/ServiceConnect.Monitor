define(["backbone", "sinon", 'app/views/navigation', 'jquery', 'underscore'], function (Backbone, sinon, Navigation, $, _) {

    describe("Navigation View", function () {

        var view, spy, htmlArgs;

        describe("Render", function() {

            beforeEach(function () {
                view = new Navigation();
                htmlArgs = [];
                spy = sinon.stub($.prototype, "html", function (a) {
                    htmlArgs.push(a);
                    return sinon.stub();
                });
                view.render();
            });

            it("should add html template to page", function () {
                expect($.prototype.html.called).toBeTruthy();
            });


            afterEach(function() {
                $.prototype.html.restore();
            });
        });

        describe("SetActive", function () {

            beforeEach(function () {
                view = new Navigation();
                htmlArgs = [];
                spy = sinon.stub($.prototype, "html", function (a) {
                    htmlArgs.push(a);
                    return sinon.stub();
                });
                view.render();
                view.setActive("endpoints");
            });

            it("should set all navigation items to inactive and set the the endpoints navigation item to active", function () {
                expect($.prototype.html.called).toBeTruthy();
            });

            afterEach(function () {
                $.prototype.html.restore();
            });
        });
            
    });
});