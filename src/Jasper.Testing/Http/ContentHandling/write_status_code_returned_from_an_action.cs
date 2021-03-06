﻿using System.Reflection;
using System.Threading.Tasks;
using Baseline.Reflection;
using Jasper.Http;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
{
    public class write_status_code_returned_from_an_action : RegistryContext<HttpTestingApp>
    {
        [Fact]
        public Task set_status_from_sync_action()
        {
            return scenario(_ =>
            {
                _.Get.Url("/status1");
                _.StatusCodeShouldBe(201);
            });
        }

        [Fact]
        public Task set_status_from_async_action()
        {
            return scenario(_ =>
            {
                _.Get.Url("/status2");
                _.StatusCodeShouldBe(203);
            });
        }

        [Fact]
        public void sync_int_returning_action_is_action_candidate()
        {
            var method = typeof(StatusCodeEndpoint).GetMethod(nameof(StatusCodeEndpoint.get_status1),
                BindingFlags.Public | BindingFlags.Static);

            HttpSettings.IsCandidate(method).ShouldBeTrue();

        }


        [Fact]
        public void async_int_returning_action_is_action_candidate()
        {
            var method = ReflectionHelper.GetMethod<StatusCodeEndpoint>(x => x.get_status2());
            HttpSettings.IsCandidate(method).ShouldBeTrue();
        }

        public write_status_code_returned_from_an_action(RegistryFixture<HttpTestingApp> fixture) : base(fixture)
        {
        }
    }

    public class StatusCodeEndpoint
    {
        public static int get_status1()
        {
            return 201;
        }

        public Task<int> get_status2()
        {
            return Task.FromResult(203);
        }
    }
}
