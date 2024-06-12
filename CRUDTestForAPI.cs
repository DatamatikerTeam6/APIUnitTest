using DogRallyAPI.Controllers;
using DogRallyAPI.Data;
using DogRallyAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRUDTestDogRallyAPI
{
    [TestClass]
    public class TracksControllerTests
    {
        private TracksController _controller;
        private DogRallyContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DogRallyContext>()
                .UseInMemoryDatabase(databaseName: "TestDogRallyDatabase")
                .Options;
            _context = new DogRallyContext(options);

            // Seed the in-memory database
            SeedDatabase();

            _controller = new TracksController(_context);
            _controller.ModelState.Clear(); // Ensure ModelState is clear
        }

        private void SeedDatabase()
        {
            // Ensure the database is clean before seeding
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var exercises = new List<Exercise>
    {
        new Exercise
        {
            ExerciseID = 1, // Ensure IDs are set
            ExerciseName = "Højresving",
            ExerciseMovementEnumID = PaceEnum.Walk,
            ExerciseSideShift = false,
            ExerciseIllustrationPath = "/images/exercises/3.png",
            ExerciseClassEnumID = ClassEnum.Beginner,
            ExerciseSignNumber = 3,
            ExercisePositionX = 10,
            ExercisePositionY = 75
        },
        new Exercise
        {
            ExerciseID = 2, // Ensure IDs are set
            ExerciseName = "Venstresving",
            ExerciseMovementEnumID = PaceEnum.Walk,
            ExerciseSideShift = false,
            ExerciseIllustrationPath = "/images/exercises/4.png",
            ExerciseClassEnumID = ClassEnum.Beginner,
            ExerciseSignNumber = 4,
            ExercisePositionX = 10,
            ExercisePositionY = 150
        }
    };

            var tracks = new List<Track>
    {
        new Track { TrackID = 1, TrackName = "Track 1", TrackDate = DateTime.Now, UserID = "user1" },
        new Track { TrackID = 2, TrackName = "Track 2", TrackDate = DateTime.Now, UserID = "user2" }
    };

            var trackExercises = new List<TrackExercise>
    {
        new TrackExercise { ForeignTrackID = 1, ForeignExerciseID = 1, TrackExercisePositionX = 10, TrackExercisePositionY = 20 },
        new TrackExercise { ForeignTrackID = 1, ForeignExerciseID = 2, TrackExercisePositionX = 30, TrackExercisePositionY = 40 },
        new TrackExercise { ForeignTrackID = 2, ForeignExerciseID = 1, TrackExercisePositionX = 15, TrackExercisePositionY = 25 },
        new TrackExercise { ForeignTrackID = 2, ForeignExerciseID = 2, TrackExercisePositionX = 35, TrackExercisePositionY = 45 }
    };

            var trackExerciseDTOs = new List<TrackExerciseDTO>
    {
        new TrackExerciseDTO { ForeignTrackID = 1, ForeignExerciseID = 1, TrackExercisePositionX = 10, TrackExercisePositionY = 20 },
        new TrackExerciseDTO { ForeignTrackID = 1, ForeignExerciseID = 2, TrackExercisePositionX = 30, TrackExercisePositionY = 40 },
        new TrackExerciseDTO { ForeignTrackID = 2, ForeignExerciseID = 1, TrackExercisePositionX = 15, TrackExercisePositionY = 25 },
        new TrackExerciseDTO { ForeignTrackID = 2, ForeignExerciseID = 2, TrackExercisePositionX = 35, TrackExercisePositionY = 45 }
    };

            _context.Exercises.AddRange(exercises);
            _context.Tracks.AddRange(tracks);
            _context.TrackExercises.AddRange(trackExercises);
            _context.TrackExerciseDTOS.AddRange(trackExerciseDTOs);
            _context.SaveChanges();
        }



        [TestMethod]
        public async Task CreateTrackReturnsOK()
        {
            // Arrange
            var viewModel = new TrackExerciseViewModelDTO
            {
                Track = new TrackDTO { TrackID = 3, TrackName = "Track 2", TrackDate = DateTime.Now, UserID = "user1" },
                Exercises = new List<ExerciseDTO>
                {
                    new ExerciseDTO { ExerciseID = 1, ExercisePositionX = 10, ExercisePositionY = 20 },
                    new ExerciseDTO { ExerciseID = 2, ExercisePositionX = 30, ExercisePositionY = 40 }
                }
            };

            // Act
            var result = await _controller.CreateTrack(viewModel);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }
        [TestMethod]
        public async Task ReadTrackReturnsOK()
        {
            // Arrange
            int trackId = 2;

            // Act
            var result = await _controller.ReadTrack(trackId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            // Verify the track exercises were returned
            var trackExercises = okResult.Value as List<TrackExerciseDTO>;
            Assert.IsNotNull(trackExercises);
            Assert.AreEqual(2, trackExercises.Count); // Expect 2 exercises based on seed data
        }

        [TestMethod]
        public async Task UpdateTrackReturnsOK()
        {
            // Arrange
            var viewModel = new TrackExerciseViewModelDTO
            {
                Track = new TrackDTO { TrackID = 2, TrackName = "Updated Track 1", TrackDate = DateTime.Now, UserID = "user1" },
                Exercises = new List<ExerciseDTO>
                {
                    new ExerciseDTO { ExerciseID = 1, ExercisePositionX = 50, ExercisePositionY = 60 },
                    new ExerciseDTO { ExerciseID = 2, ExercisePositionX = 70, ExercisePositionY = 80 }
                }
            };

            // Act
            var result = await _controller.UpdateTrack(viewModel);

            // Assert
            var okResult = result as OkResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            // Verify the track was updated
            var track = await _context.Tracks
                .Include(t => t.TrackExercises)
                .FirstOrDefaultAsync(t => t.TrackID == viewModel.Track.TrackID);
            Assert.IsNotNull(track);
            Assert.AreEqual("Updated Track 1", track.TrackName);
            Assert.AreEqual(2, track.TrackExercises.Count);
        }

        [TestMethod]
        public async Task DeleteTrackReturnsOK()
        {
            // Arrange
            int trackId = 1;

            // Act
            var result = await _controller.DeleteTrack(trackId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual($"Track with ID {trackId} has been deleted.", okResult.Value);

            // Verify the track was deleted
            var track = await _context.Tracks.FindAsync(trackId);
            Assert.IsNull(track);
        }
    }
}
